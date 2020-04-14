using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SosCafe.Admin.Models.Api;
using SosCafe.Admin.Models.Queue;

namespace SosCafe.Admin
{
    public static class VendorCreation
    {
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
