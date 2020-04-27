using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SosCafe.Admin.Models.Api;
using System.Web.Http;
using System;
using System.Security.Claims;
using SosCafe.Admin.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SosCafe;
using SosCafe.Admin.Csv;
using SosCafe.Admin.Models.Queue;

namespace SosCafe.Admin
{
    public static class VendorManagement
    {
        internal static readonly Regex BankAccountRegex = new Regex(@"[0-9]{2}[- ]?[0-9]{4}[- ]?[0-9]{7}[- ]?[0-9]{2,3}");
        private static string[] AllowedCities = new string[]
        {
            "Auckland/Central",
            "Auckland/North",
            "Auckland/South",
            "Auckland/East",
            "Auckland/West",
            "Bay Of Plenty",
            "Canterbury",
            "Central North Island",
            "Christchurch",
            "Coromandel",
            "Dunedin",
            "Gisborne",
            "Hamilton",
            "Hawke's Bay",
            "Kapiti Coast",
            "Manawatu/Whanganui",
            "Nelson/Marlborough",
            "Northland",
            "North Canterbury",
            "Otago",
            "Queenstown",
            "Southland",
            "South Canterbury",
            "Taranaki",
            "Waikato",
            "Wairarapa",
            "Wellington",
            "Wellington/Hutt Valley",
            "West Coast"
        };
        private static string[] AllowedTypes = new string[]
        {
            "Food/Beverage",
            "Service"
        };

        [FunctionName("GetVendor")]
        public static async Task<IActionResult> GetVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendors/{vendorId}")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("Vendors", "Vendors", "{vendorId}", Connection = "SosCafeStorage")] VendorDetailsEntity vendorDetailsEntity,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Authorise the request.
            log.LogInformation("Received GET vendor request for vendor {VendorId}.", vendorId);
            if (!await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, claimsPrincipal, vendorId))
            {
                log.LogInformation("Received unauthorised request for vendor {VendorId}. Denying request.", vendorId);
                return new NotFoundResult();
            }

            // Map to an API response.
            var vendorDetailsResponse = new VendorDetailsApiModel
            {
                Id = vendorDetailsEntity.ShopifyId,
                RegisteredDate = vendorDetailsEntity.RegisteredDate,
                BusinessName = vendorDetailsEntity.BusinessName,
                ContactName = vendorDetailsEntity.ContactName,
                EmailAddress = vendorDetailsEntity.EmailAddress,
                PhoneNumber = vendorDetailsEntity.PhoneNumber,
                BankAccountNumber = vendorDetailsEntity.BankAccountNumber,
                HasAgreedToTerms = (vendorDetailsEntity.DateAcceptedTerms != null),
                IsClickAndCollect = vendorDetailsEntity.IsClickAndCollect,
                Level1Closed = vendorDetailsEntity.Level1Closed,
                Level1Delivery = vendorDetailsEntity.Level1Delivery,
                Level1ClickAndCollect = vendorDetailsEntity.Level1ClickAndCollect,
                Level1Open = vendorDetailsEntity.Level1Open,
                Level2Closed = vendorDetailsEntity.Level2Closed,
                Level2Delivery = vendorDetailsEntity.Level2Delivery,
                Level2ClickAndCollect = vendorDetailsEntity.Level2ClickAndCollect,
                Level2Open = vendorDetailsEntity.Level2Open,
                Level3Closed = vendorDetailsEntity.Level3Closed,
                Level3Delivery = vendorDetailsEntity.Level3Delivery,
                Level3ClickAndCollect = vendorDetailsEntity.Level3ClickAndCollect,
                Level3Open = vendorDetailsEntity.Level3Open,
                ClickAndCollectUrl = vendorDetailsEntity.ClickAndCollectUrl
            };

            // Return the vendor details.
            return new OkObjectResult(vendorDetailsResponse);
        }

        [FunctionName("UpdateVendor")]
        public static async Task<IActionResult> UpdateVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "vendors/{vendorId}")] UpdateVendorDetailsApiModel vendorDetailsApiModel,
            HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("Vendors", "Vendors", "{vendorId}", Connection= "SosCafeStorage")] VendorDetailsEntity vendorDetailsEntity,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Authorise the request.
            log.LogInformation("Received PUT vendors request for vendor {VendorId}.", vendorId);
            if (! await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, claimsPrincipal, vendorId))
            {
                log.LogInformation("Received unauthorised request for vendor {VendorId}. Denying request.", vendorId);
                return new NotFoundResult();
            }

            // Perform validation on the properties.
            if (vendorDetailsEntity.DateAcceptedTerms == null && vendorDetailsApiModel.DateAcceptedTerms == null)
            {
                return new BadRequestErrorMessageResult("The terms must be accepted in order to update the vendor.");
            }
            else if (!BankAccountRegex.IsMatch(vendorDetailsApiModel.BankAccountNumber))
            {
                return new BadRequestErrorMessageResult("The bank account number is invalid.");
            }

            // Update entity.
            vendorDetailsEntity.ContactName = vendorDetailsApiModel.ContactName;
            vendorDetailsEntity.PhoneNumber = vendorDetailsApiModel.PhoneNumber;
            vendorDetailsEntity.BankAccountNumber = vendorDetailsApiModel.BankAccountNumber;
            vendorDetailsEntity.IsClickAndCollect = vendorDetailsApiModel.IsClickAndCollect;
            vendorDetailsEntity.Level1Closed = vendorDetailsApiModel.Level1Closed;
            vendorDetailsEntity.Level1Delivery = vendorDetailsApiModel.Level1Delivery;
            vendorDetailsEntity.Level1ClickAndCollect = vendorDetailsApiModel.Level1ClickAndCollect;
            vendorDetailsEntity.Level1Open = vendorDetailsApiModel.Level1Open;
            vendorDetailsEntity.Level2Closed = vendorDetailsApiModel.Level2Closed;
            vendorDetailsEntity.Level2Delivery = vendorDetailsApiModel.Level2Delivery;
            vendorDetailsEntity.Level2ClickAndCollect = vendorDetailsApiModel.Level2ClickAndCollect;
            vendorDetailsEntity.Level2Open = vendorDetailsApiModel.Level2Open;
            vendorDetailsEntity.Level3Closed = vendorDetailsApiModel.Level3Closed;
            vendorDetailsEntity.Level3Delivery = vendorDetailsApiModel.Level3Delivery;
            vendorDetailsEntity.Level3ClickAndCollect = vendorDetailsApiModel.Level3ClickAndCollect;
            vendorDetailsEntity.Level3Open = vendorDetailsApiModel.Level3Open;
            vendorDetailsEntity.ClickAndCollectUrl = vendorDetailsApiModel.ClickAndCollectUrl;
            if (vendorDetailsApiModel.DateAcceptedTerms != null)
            {
                vendorDetailsEntity.DateAcceptedTerms = vendorDetailsApiModel.DateAcceptedTerms.Value.ToString("o");
            }

            // Submit entity update to table.
            var replaceVendorDetailsEntityOperation = TableOperation.Replace(vendorDetailsEntity);
            var replaceVendorDetailsEntityOperationResult = await vendorDetailsTable.ExecuteAsync(replaceVendorDetailsEntityOperation);
            if (replaceVendorDetailsEntityOperationResult.HttpStatusCode < 200 || replaceVendorDetailsEntityOperationResult.HttpStatusCode > 299)
            {
                log.LogError("Failed to replace entity in Vendors table. Status code={InsertStatusCode}, Result={InsertResult}", replaceVendorDetailsEntityOperationResult.HttpStatusCode, replaceVendorDetailsEntityOperationResult.Result);
                return new InternalServerErrorResult();
            }
            else
            {
                log.LogInformation("Replaced entity in Vendors table.");
                return new OkResult();
            }
        }

        [FunctionName("GetVendorPayments")]
        public static async Task<IActionResult> GetVendorPayments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendors/{vendorId}/payments")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("VendorPayments", Connection = "SosCafeStorage")] CloudTable vendorPaymentsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Authorise the request.
            log.LogInformation("Received GET payments request for vendor {VendorId}.", vendorId);
            if (!await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, claimsPrincipal, vendorId))
            {
                log.LogInformation("Received unauthorised request for vendor {VendorId}. Denying request.", vendorId);
                return new NotFoundResult();
            }

            // Get the payments and map the results to a response model.
            var allPaymentsForVendor = await GetPaymentsForVendorAsync(vendorId, vendorPaymentsTable);
            var mappedResults = allPaymentsForVendor.Select(entity => new VendorPaymentApiModel
            {
                PaymentId = entity.PaymentId,
                PaymentDate = entity.PaymentDate,
                BankAccountNumber = entity.BankAccountNumber,
                GrossPayment = entity.GrossPayment,
                Fees = entity.Fees,
                NetPayment = entity.NetPayment
            })
                .OrderByDescending(d => d.PaymentDate);

            // Return the payment list.
            return new OkObjectResult(mappedResults);
        }

        [FunctionName("ExportVendorPayments")]
        public static async Task<IActionResult> ExportVendorPayments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendors/{vendorId}/payments/csv")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("VendorPayments", Connection = "SosCafeStorage")] CloudTable vendorPaymentsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Authorise the request.
            log.LogInformation("Received GET payments CSV request for vendor {VendorId}.", vendorId);
            if (!await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, claimsPrincipal, vendorId))
            {
                log.LogInformation("Received unauthorised request for vendor {VendorId}. Denying request.", vendorId);
                return new NotFoundResult();
            }

            // Get the payments and map the results to a response model.
            var allPaymentsForVendor = await GetPaymentsForVendorAsync(vendorId, vendorPaymentsTable);
            var mappedResults = allPaymentsForVendor.Select(entity => new VendorPaymentCsv
            {
                VendorId = entity.VendorId,
                PaymentId = entity.PaymentId,
                PaymentDate = entity.PaymentDate,
                BankAccountNumber = entity.BankAccountNumber,
                NetPayment = entity.NetPayment.ToString()
            })
                .OrderByDescending(d => d.PaymentDate);

            // Serialize to CSV.
            var fileBytes = CsvCreator.CreateCsvFile(mappedResults);
            return new FileContentResult(fileBytes, "text/csv")
            {
                FileDownloadName = "SOSCafe-Payments.csv"
            };
        }

        [FunctionName("GetVendorVouchers")]
        public static async Task<IActionResult> GetVendorVouchers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendors/{vendorId}/vouchers")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("VendorVouchers", Connection = "SosCafeStorage")] CloudTable vendorVouchersTable,
            [Table("VendorVoucherRedemptions", Connection = "SosCafeStorage")] CloudTable vendorVoucherRedemptionsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Authorise the request.
            log.LogInformation("Received GET vouchers request for vendor {VendorId}.", vendorId);
            if (!await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, claimsPrincipal, vendorId))
            {
                log.LogInformation("Received unauthorised request for vendor {VendorId}. Denying request.", vendorId);
                return new NotFoundResult();
            }

            // Get the vouchers and map the results to a response model.
            var (allVouchers, allVoucherRedemptions) = await GetVouchersForVendorAsync(vendorId, vendorVouchersTable, vendorVoucherRedemptionsTable);
            var mappedResults = allVouchers.Select(entity => new VendorVoucherApiModel
            {
                LineItemId = entity.LineItemId,
                OrderId = entity.OrderId,
                OrderRef = entity.OrderRef,
                OrderDate = entity.OrderDate,
                CustomerName = entity.CustomerName,
                CustomerRegion = entity.CustomerRegion,
                CustomerEmailAddress = entity.CustomerEmailAddress,
                CustomerAcceptsMarketing = entity.CustomerAcceptsMarketing,
                VoucherId = entity.VoucherId,
                VoucherDescription = entity.VoucherDescription,
                VoucherQuantity = entity.VoucherQuantity,
                VoucherIsDonation = entity.VoucherIsDonation,
                VoucherGross = entity.VoucherGross,
                VoucherFees = entity.VoucherFees,
                VoucherNet = entity.VoucherNet,
                RedemptionDate = allVoucherRedemptions.SingleOrDefault(redemptionEntity => redemptionEntity.LineItemId == entity.LineItemId)?.RedemptionDate
            })
                .OrderByDescending(d => d.OrderDate);

            // Return the voucher list.
            return new OkObjectResult(mappedResults);
        }

        [FunctionName("ExportVendorVouchers")]
        public static async Task<IActionResult> ExportVendorVouchers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendors/{vendorId}/vouchers/csv")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("VendorVouchers", Connection = "SosCafeStorage")] CloudTable vendorVouchersTable,
            [Table("VendorVoucherRedemptions", Connection = "SosCafeStorage")] CloudTable vendorVoucherRedemptionsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Authorise the request.
            log.LogInformation("Received GET vouchers CSV request for vendor {VendorId}.", vendorId);
            if (!await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, claimsPrincipal, vendorId))
            {
                log.LogInformation("Received unauthorised request for vendor {VendorId}. Denying request.", vendorId);
                return new NotFoundResult();
            }

            // Get the vouchers and map the results to a response model.
            var (allVouchers, allVoucherRedemptions) = await GetVouchersForVendorAsync(vendorId, vendorVouchersTable, vendorVoucherRedemptionsTable);
            var mappedResults = allVouchers.Select(entity => new VendorVoucherCsv
            {
                VendorId = entity.VendorId,
                LineItemId = entity.LineItemId,
                OrderId = entity.OrderId,
                OrderRef = entity.OrderRef,
                OrderDate = entity.OrderDate,
                CustomerName = entity.CustomerName,
                CustomerRegion = entity.CustomerRegion,
                CustomerEmailAddress = entity.CustomerEmailAddress,
                CustomerAcceptsMarketing = entity.CustomerAcceptsMarketing.ToString(),
                VoucherId = entity.VoucherId,
                VoucherDescription = entity.VoucherDescription,
                VoucherQuantity = entity.VoucherQuantity,
                VoucherIsDonation = entity.VoucherIsDonation.ToString(),
                VoucherGross = entity.VoucherGross.ToString(),
                VoucherFees = entity.VoucherFees.ToString(),
                VoucherNet = entity.VoucherNet.ToString(),
                RedemptionDate = allVoucherRedemptions.SingleOrDefault(redemptionEntity => redemptionEntity.LineItemId == entity.LineItemId)?.RedemptionDate
            })
                .OrderByDescending(d => d.OrderDate);

            // Serialize to CSV.
            var fileBytes = CsvCreator.CreateCsvFile(mappedResults);
            return new FileContentResult(fileBytes, "text/csv")
            {
                FileDownloadName = "SOSCafe-Vouchers.csv"
            };
        }

        [FunctionName("UpdateVoucher")]
        public static async Task<IActionResult> UpdateVoucher(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PATCH", Route = "vendors/{vendorId}/vouchers/{voucherId}")] UpdateVoucherApiModel updateVoucherApiModel,
            ClaimsPrincipal claimsPrincipal,
            string vendorId, string voucherId,
            [Table("VendorVoucherRedemptions", Connection = "SosCafeStorage")] CloudTable vendorVoucherRedemptionsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Authorise the request.
            log.LogInformation("Received PATCH vouchers request for vendor {VendorId} and voucher ID {VoucherId}.", vendorId, voucherId);
            if (!await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, claimsPrincipal, vendorId))
            {
                log.LogInformation("Received unauthorised request for vendor {VendorId}. Denying request.", vendorId);
                return new NotFoundResult();
            }

            // Create a voucher redemption entity so that we can track it.
            var voucherRedemptionEntity = new VendorVoucherRedemptionEntity
            {
                VendorId = vendorId,
                LineItemId = voucherId
            };

            if (updateVoucherApiModel.RedemptionDate == null)
            {
                // Delete the redemption.
                var deleteVoucherRedemptionEntityOperation = TableOperation.Delete(voucherRedemptionEntity);
                var deleteVoucherRedemptionEntityOperationResult = await vendorVoucherRedemptionsTable.ExecuteAsync(deleteVoucherRedemptionEntityOperation);
                if (deleteVoucherRedemptionEntityOperationResult.HttpStatusCode == 404)
                {
                    log.LogInformation("Did not found a VendorVoucherRedemptions entity to delete.");
                }
                else if (deleteVoucherRedemptionEntityOperationResult.HttpStatusCode < 200 || deleteVoucherRedemptionEntityOperationResult.HttpStatusCode > 299)
                {
                    log.LogError("Failed to delete entity from VendorVoucherRedemptions table. Status code={UpsertStatusCode}, Result={DeleteResult}", deleteVoucherRedemptionEntityOperationResult.HttpStatusCode, deleteVoucherRedemptionEntityOperationResult.Result);
                }
                else
                {
                    log.LogInformation("Deleted entity from VendorVoucherRedemptions table.");
                }
            }
            else
            {
                // Note the redemption date on the voucher.
                voucherRedemptionEntity.RedemptionDate = updateVoucherApiModel.RedemptionDate.Value;

                // Create or update the redemption.
                var upsertVoucherRedemptionEntityOperation = TableOperation.InsertOrReplace(voucherRedemptionEntity);
                var upsertVoucherRedemptionEntityOperationResult = await vendorVoucherRedemptionsTable.ExecuteAsync(upsertVoucherRedemptionEntityOperation);
                if (upsertVoucherRedemptionEntityOperationResult.HttpStatusCode < 200 || upsertVoucherRedemptionEntityOperationResult.HttpStatusCode > 299)
                {
                    log.LogError("Failed to upsert entity into VendorVoucherRedemptions table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertVoucherRedemptionEntityOperationResult.HttpStatusCode, upsertVoucherRedemptionEntityOperationResult.Result);
                }
                else
                {
                    log.LogInformation("Upserted entity into VendorVoucherRedemptions table.");
                }
            }

            // Return the voucher list.
            return new OkResult();
        }

        [FunctionName("CreateVendor")]
        public static IActionResult CreateVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "vendors")] AddVendorRequestApiModel requestModel,
            HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            [Queue("addvendor", Connection = "SosCafeStorage")] ICollector<AddVendorQueueModel> outputMessages,
            ILogger log)
        {
            // Get the user details from their token claims.
            // This also implicitly authorizes the request, since any authenticated user can access this API.
            var emailAddress = UserManagement.GetEmailAddress(claimsPrincipal);
            var contactName = UserManagement.GetDisplayName(claimsPrincipal);
            if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(contactName))
            {
                log.LogError("Token is missing required claims.");
                return new UnauthorizedResult();
            }

            var registrationTime = DateTime.UtcNow;

            // Perform validation on the properties.
            if (requestModel.DateAcceptedTerms == null)
            {
                return new BadRequestErrorMessageResult("The terms must be accepted in order to update the vendor.");
            }
            if (!BankAccountRegex.IsMatch(requestModel.BankAccountNumber))
            {
                return new BadRequestErrorMessageResult("The bank account number is invalid.");
            }
            if (!AllowedCities.Contains(requestModel.City))
            {
                return new BadRequestErrorMessageResult("The city is invalid.");
            }
            if (!AllowedTypes.Contains(requestModel.Type))
            {
                return new BadRequestErrorMessageResult("The type is invalid.");
            }

            // Enqueue a message for the logic app to process.
            outputMessages.Add(new AddVendorQueueModel
            {
                BusinessName = requestModel.BusinessName,
                Type = requestModel.Type,
                PhoneNumber = requestModel.PhoneNumber,
                Description = requestModel.Description,
                City = requestModel.City,
                BusinessPhotoUrl = requestModel.BusinessPhotoUrl,
                BankAccountNumber = requestModel.BankAccountNumber,
                IsClickAndCollect = requestModel.IsClickAndCollect,
                DateAcceptedTerms = registrationTime,
                EmailAddress = emailAddress,
                EmailAddressCanonicalised = emailAddress.ToUpper().CleanStringForPartitionKey(),
                ContactName = contactName,
                RegisteredDate = registrationTime,
                Level1Closed = requestModel.Level1Closed,
                Level1Delivery = requestModel.Level1Delivery,
                Level1ClickAndCollect = requestModel.Level1ClickAndCollect,
                Level1Open = requestModel.Level1Open,
                Level2Closed = requestModel.Level2Closed,
                Level2Delivery = requestModel.Level2Delivery,
                Level2ClickAndCollect = requestModel.Level2ClickAndCollect,
                Level2Open = requestModel.Level2Open,
                Level3Closed = requestModel.Level3Closed,
                Level3Delivery = requestModel.Level3Delivery,
                Level3ClickAndCollect = requestModel.Level3ClickAndCollect,
                Level3Open = requestModel.Level3Open,
                ClickAndCollectUrl = requestModel.ClickAndCollectUrl ?? string.Empty
            });

            return new AcceptedResult();
        }

        private static async Task<List<VendorPaymentEntity>> GetPaymentsForVendorAsync(string vendorId, CloudTable vendorPaymentsTable)
        {
            // Read all records from table storage where the partition key is the vendor's ID.
            TableContinuationToken token = null;
            var allPaymentsForVendor = new List<VendorPaymentEntity>();
            var filterToVendorPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, vendorId);
            do
            {
                var queryResult = await vendorPaymentsTable.ExecuteQuerySegmentedAsync(new TableQuery<VendorPaymentEntity>().Where(filterToVendorPartition), token);
                allPaymentsForVendor.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return allPaymentsForVendor;
        }

        private static async Task<(List<VendorVoucherEntity>, List<VendorVoucherRedemptionEntity>)> GetVouchersForVendorAsync(string vendorId, CloudTable vendorVouchersTable, CloudTable vendorVoucherRedemptionsTable)
        {
            var voucherEntitiesTask = GetAllEntitiesForVendorAsync<VendorVoucherEntity>(vendorId, vendorVouchersTable);
            var voucherRedemptionEntitiesTask = GetAllEntitiesForVendorAsync<VendorVoucherRedemptionEntity>(vendorId, vendorVoucherRedemptionsTable);

            await Task.WhenAll(voucherEntitiesTask, voucherRedemptionEntitiesTask);

            return (voucherEntitiesTask.Result, voucherRedemptionEntitiesTask.Result);
        }

        private static async Task<List<T>> GetAllEntitiesForVendorAsync<T>(string vendorId, CloudTable table) where T : ITableEntity, new()
        {
            // Read all records from table storage where the partition key is the vendor's ID.
            TableContinuationToken token = null;
            var allEntitiesForVendor = new List<T>();
            var filterToVendorPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, vendorId);
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<T>().Where(filterToVendorPartition), token);
                allEntitiesForVendor.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return allEntitiesForVendor;
        }
    }
}
