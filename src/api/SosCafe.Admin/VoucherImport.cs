using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using ShopifySharp;
using ShopifySharp.Filters;
using SosCafe.Admin.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SosCafe.Admin
{
    public static class VoucherImport
    {
        private static string ShopifyDomainName = Environment.GetEnvironmentVariable("ShopifyDomainName");
        private static string ShopifyPassword = Environment.GetEnvironmentVariable("ShopifyPassword");
        private static OrderService _orderService = new OrderService(ShopifyDomainName, ShopifyPassword);
        private const int LimitPerPage = 250; // This is the maximum limit for the Shopify API.
        private const string FieldList = "buyer_accepts_marketing,customer,line_items,id,created_at,updated_at,name,refunds,gateway";

        [FunctionName("ImportVouchers")]
        public static async Task ImportVouchers([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [Blob("shopify-vouchers/current-status", FileAccess.ReadWrite, Connection = "SosCafeStorage")] CloudBlockBlob statusBlob,
            [Table("ShopifyVouchers", Connection = "SosCafeStorage")] CloudTable shopifyVouchersTable,
            ILogger log)
        {
            string continuationToken = null;
            if (await statusBlob.ExistsAsync())
            {
                continuationToken = await statusBlob.DownloadTextAsync();
            }

            var newContinuationToken = await ProcessOrders(shopifyVouchersTable, log, continuationToken: continuationToken);
            await statusBlob.UploadTextAsync(newContinuationToken);
        }

        public class MyCustomOrderListFilter : OrderListFilter
        {
            [JsonProperty("order")]
            public string Order { get; set; }
        }

        private static async Task<string> ProcessOrders(CloudTable shopifyVouchersTable, ILogger log, int ordersToProcess = 1000, string continuationToken = null)
        {
            var numberOrdersProcessed = 0;
            DateTimeOffset? updatedAtHighWaterMark = continuationToken == null ? new DateTimeOffset(new DateTime(2020, 1, 1)) : DateTimeOffset.Parse(continuationToken);
            ListFilter<Order> filter = new MyCustomOrderListFilter
            {
                CreatedAtMin = updatedAtHighWaterMark,
                Limit = LimitPerPage,
                Status = "any",
                Fields = FieldList,
                Order = "updated_at asc"
            };

            var voucherEntities = new List<VendorVoucherEntity>();

            while (numberOrdersProcessed < ordersToProcess)
            {
                // Read this page of results.
                var responsePage = await _orderService.ListAsync(filter);
                log.LogInformation($"Read {responsePage.Items.Count()} orders from Shopify API.");
                numberOrdersProcessed += responsePage.Items.Count();

                // Process the page.
                if (responsePage.Items.Any())
                {
                    // Convert the order line items into vouchers.
                    foreach (var order in responsePage.Items)
                    {
                        voucherEntities.AddRange(MapShopifyOrderToEntities(order));
                    }

                    // Keep track of the highest date we've received.
                    var pageUpdatedAtHighWaterMark = responsePage.Items.Max(i => i.UpdatedAt).Value;
                    if (pageUpdatedAtHighWaterMark > updatedAtHighWaterMark)
                    {
                        updatedAtHighWaterMark = pageUpdatedAtHighWaterMark;
                    }
                }

                // Check if we are done with processing this batch.
                if (numberOrdersProcessed >= ordersToProcess || !responsePage.HasNextPage)
                {
                    break;
                }

                // We have more to read in this batch, so pause for 1s and then load the next page.
                filter = responsePage.LinkHeader.NextLink.GetFollowingPageFilter(limit: LimitPerPage, fields: FieldList);
                await Task.Delay(200);
            }

            // Insert the vouchers into the table.
            foreach (var voucherEntity in voucherEntities)
            {
                var upsertShopifyVoucherEntityOperation = TableOperation.InsertOrReplace(voucherEntity);
                var upsertShopifyVoucherEntityOperationResult = await shopifyVouchersTable.ExecuteAsync(upsertShopifyVoucherEntityOperation);
                if (upsertShopifyVoucherEntityOperationResult.HttpStatusCode < 200 || upsertShopifyVoucherEntityOperationResult.HttpStatusCode > 299)
                {
                    log.LogError("Failed to upsert entity into ShopifyVouchers table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertShopifyVoucherEntityOperationResult.HttpStatusCode, upsertShopifyVoucherEntityOperationResult.Result);
                }
                else
                {
                    log.LogInformation("Upserted entity into ShopifyVouchers table.");
                }
            }

            log.LogInformation($"The continuation token should now be set to {updatedAtHighWaterMark}.");
            return updatedAtHighWaterMark.ToString();
        }

        public static List<VendorVoucherEntity> MapShopifyOrderToEntities(Order order)
        {
            var entities = new List<VendorVoucherEntity>();

            foreach (var lineItem in order.LineItems)
            {
                var entity = new VendorVoucherEntity
                {
                    CustomerAcceptsMarketing = order.BuyerAcceptsMarketing ?? false,
                    CustomerEmailAddress = order.Customer.Email,
                    CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                    CustomerRegion = order.Customer.DefaultAddress.City,
                    LineItemId = lineItem.Id.Value.ToString(),
                    OrderDate = order.CreatedAt.Value.UtcDateTime,
                    OrderId = order.Id.Value.ToString(),
                    OrderRef = order.Name,
                    Gateway = order.Gateway,
                    VendorId = lineItem.ProductId.ToString(),
                    VoucherDescription = lineItem.VariantTitle,
                    VoucherId = lineItem.Properties.SingleOrDefault(p => string.Equals(p.Name.ToString(), "Voucher id", StringComparison.InvariantCultureIgnoreCase))?.Value.ToString() ?? string.Empty,
                    VoucherQuantity = lineItem.Quantity.Value,
                    VoucherType = lineItem.Properties.SingleOrDefault(p => string.Equals(p.Name.ToString(), "Type", StringComparison.InvariantCultureIgnoreCase))?.Value.ToString(),
                };

                // We determine if the voucher has been refunded by seeing if there are any line items matching this line item ID within the order's refunds collection.
                entity.IsRefunded = order.Refunds.Any(r => r.RefundLineItems.Any(rli => rli.LineItemId == lineItem.Id));

                // We calculate the fees separately using specific logic.
                entity.VoucherGross = decimal.Round(lineItem.Price.Value * lineItem.Quantity.Value, 2);
                entity.VoucherFees = CalculateFees(order.LineItems.Count(), order.Gateway, entity.VoucherGross);
                entity.VoucherNet = entity.VoucherGross - entity.VoucherFees;

                entities.Add(entity);
            }

            return entities;
        }

        private static decimal CalculateFees(int numberLineItemsInOrder, string paymentGatewayName, decimal voucherGross)
        {
            decimal percentageFees;
            switch (paymentGatewayName)
            {
                case "shopify_payments":
                    percentageFees = voucherGross * 0.02M;
                    break;
                case "poli_internet_banking":
                    percentageFees = voucherGross * 0.01M;
                    break;
                default:
                    percentageFees = voucherGross * 0.034M;
                    break;
            }

            decimal fixedFees;
            switch (paymentGatewayName)
            {
                case "shopify_payments":
                    fixedFees = (0.3M / numberLineItemsInOrder);
                    break;
                case "poli_internet_banking":
                    fixedFees = 0;
                    break;
                default:
                    fixedFees = (0.45M / numberLineItemsInOrder);
                    break;
            }

            return decimal.Round(decimal.Round(percentageFees, 2) + decimal.Round(fixedFees, 2), 2);
        }
    }
}
