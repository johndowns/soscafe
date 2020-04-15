using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SosCafe.Admin.Models.Api;
using SosCafe.Admin.Models.Queue;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Web.Http;
using ShopifySharp;
using System.Collections.Generic;

namespace SosCafe.Admin
{
    public static class VendorCreation
    {
        private static string ShopifyDomainName = Environment.GetEnvironmentVariable("ShopifyDomainName");
        private static string ShopifyPassword = Environment.GetEnvironmentVariable("ShopifyPassword");
        private static string SendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");
        private static string SendGridTemplateId = Environment.GetEnvironmentVariable("SendGridTemplateId");
        private static string SendGridEmailFromAddress = Environment.GetEnvironmentVariable("SendGridEmailFromAddress");
        private static string SendGridEmailFromName = Environment.GetEnvironmentVariable("SendGridEmailFromName");

        [FunctionName("AddVendorToShopify")]
        public static async Task<IActionResult> AddVendorToShopify(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] AddVendorQueueModel addVendorModel,
            HttpRequest req,
            ILogger log)
        {
            // Initialise the product definition.
            var product = new Product()
            {
                Title = $"{addVendorModel.BusinessName} - {addVendorModel.City}",
                Vendor = addVendorModel.BusinessName,
                BodyHtml = addVendorModel.Description,
                ProductType = addVendorModel.Type,
                Options = new List<ProductOption>
                {
                    new ProductOption
                    {
                        Name = "Voucher"
                    }
                },
                Tags = $"{addVendorModel.City}, {addVendorModel.Type}",
                Variants = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        Option1 = "$5.00",
                        Price = 5,
                        SKU = $"{addVendorModel.BusinessName}-5",
                        Taxable = false
                    },
                    new ProductVariant
                    {
                        Option1 = "$25.00",
                        Price = 25,
                        SKU = $"{addVendorModel.BusinessName}-25",
                        Taxable = false
                    },
                    new ProductVariant
                    {
                        Option1 = "$100.00",
                        Price = 100,
                        SKU = $"{addVendorModel.BusinessName}-100",
                        Taxable = false
                    }
                }
            };

            // Initialise the image, if there is one.
            if (!string.IsNullOrEmpty(addVendorModel.BusinessPhotoUrl))
            {
                product.Images = new List<ProductImage>
                {
                    new ProductImage
                    {
                        Src = addVendorModel.BusinessPhotoUrl
                    }
                };
            }

            // Submit the product definition to Shopify.
            var service = new ProductService(ShopifyDomainName, ShopifyPassword);
            product = await service.CreateAsync(product, new ProductCreateOptions 
            {
                Published = false
            }
            );

            // Handle the response.
            if (product?.Id == null)
            {
                log.LogError("Received unexpected empty product from Shopify.");
                return new InternalServerErrorResult();
            }

            var productId = product.Id.ToString();

            var responseObject = new AddVendorToShopifyResultApiModel() { ShopifyId = productId };
            return new OkObjectResult(responseObject);
        }

        [FunctionName("SendVendorWelcomeEmail")]
        public static async Task<IActionResult> SendVendorWelcomeEmail(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route =  null)] AddVendorQueueModel addVendorModel,
           HttpRequest req,
           ILogger log)
        {
            // Initialize the SendGrid client.
            var client = new SendGridClient(SendGridApiKey);

            // Prepare the email message.
            var emailMessage = new SendGridMessage();
            emailMessage.SetFrom(new EmailAddress(SendGridEmailFromAddress, SendGridEmailFromName));
            emailMessage.AddTo(new EmailAddress(addVendorModel.EmailAddress, addVendorModel.ContactName));
            emailMessage.SetTemplateId(SendGridTemplateId);
            
            // Send the message.
            var response = await client.SendEmailAsync(emailMessage);
            log.LogInformation("Sent mail via SendGrid and received status code {SendGridStatusCode} and headers {SendGridHeaders}.", response.StatusCode, response.Headers.ToString());

            if ((int)response.StatusCode > 299)
            {
                log.LogError("Unable to send email.");
                return new InternalServerErrorResult();
            }            
            return new OkResult();
        }
    }
}
