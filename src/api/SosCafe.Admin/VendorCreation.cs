using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SosCafe.Admin.Models.Api;
using SosCafe.Admin.Models.Queue;
using System.Security.Claims;

namespace SosCafe.Admin
{
    public static class VendorCreation
    {
        [FunctionName("CreateVendor")]
        public static async Task<IActionResult> CreateVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "vendors")] AddVendorRequestApiModel requestModel,
            HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            [Queue("addvendor", Connection = "SosCafeStorage")] ICollector<AddVendorQueueModel> outputMessages,
            ILogger log)
        {
            // TODO validate
            var registrationTime = DateTime.UtcNow;

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
                DateAcceptedTerms = registrationTime,
                UserId = "todo@todo.com",
                ContactName = "TODO",
                RegisteredDate = registrationTime
            });

            return new AcceptedResult();
        }

        [FunctionName("AddVendorToShopify")]
        public static async Task<IActionResult> AddVendorToShopify(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] AddVendorQueueModel addVendorModel,
            HttpRequest req,
            ILogger log)
        {
            // TODO send request to Shopify

            var responseObject = new AddVendorToShopifyResultApiModel() { ShopifyId = Guid.NewGuid().ToString() };
            return new OkObjectResult(responseObject);
        }

        [FunctionName("SendVendorWelcomeEmail")]
        public static async Task<IActionResult> SendVendorWelcomeEmail(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route =  null)] AddVendorQueueModel addVendorModel,
           HttpRequest req,
           ILogger log)
        {
            // TODO send email via SendGrid
            return new OkResult();
        }
    }
}
