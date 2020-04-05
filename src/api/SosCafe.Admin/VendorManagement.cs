using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SosCafe.Admin.ApiModels;

namespace SosCafe.Admin
{
    public static class VendorManagement
    {
        // TODO authorisation

        [FunctionName("GetVendor")]
        public static async Task<IActionResult> GetVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendors/{vendorId}")] HttpRequest req,
            string vendorId,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            ILogger log)
        {
            log.LogInformation("Received GET vendors request from vendor {VendorId}.", vendorId);

            // Read the vendor details from table storage.
            var findOperation = TableOperation.Retrieve<VendorDetailsEntity>("Vendors", vendorId);
            var findResult = await vendorDetailsTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                log.LogWarning("Could not find vendor {VendorId}.", vendorId);
                return new NotFoundResult();
            }
            var vendorDetailsEntity = (VendorDetailsEntity)findResult.Result;

            // Map to an API response.
            var vendorDetailsResponse = new VendorDetailsApiModel
            {
                Id = vendorDetailsEntity.ShopifyId,
                RegisteredDate = vendorDetailsEntity.RegisteredDate,
                BusinessName = vendorDetailsEntity.BusinessName,
                ContactName = vendorDetailsEntity.ContactName,
                EmailAddress = vendorDetailsEntity.EmailAddress,
                PhoneNumber = vendorDetailsEntity.PhoneNumber,
                BankAccountNumber = vendorDetailsEntity.BankAccountNumber
            };

            // Return the vendor details.
            return new OkObjectResult(vendorDetailsResponse);
        }

        [FunctionName("UpdateVendor")]
        public static async Task<IActionResult> UpdateVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "vendors/{vendorId}")] HttpRequest req,
            ILogger log)
        {
            // TODO implement this

            return new OkResult();
        }
    }
}
