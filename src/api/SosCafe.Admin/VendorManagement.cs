using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SosCafe.Admin.ApiModels;
using System.Web.Http;
using System;

namespace SosCafe.Admin
{
    public static class VendorManagement
    {
        [FunctionName("GetVendor")]
        public static async Task<IActionResult> GetVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendors/{vendorId}")] HttpRequest req,
            string vendorId,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            var userId = req.Headers["X-MS-CLIENT-PRINCIPAL-ID"];
            log.LogInformation("Received GET vendors request for vendor {VendorId} from user {UserId}.", vendorId, userId);

            // Authorise the request.
            var isAuthorised = await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, userId, vendorId);
            if (!isAuthorised)
            {
                log.LogInformation("Received unauthorised request from user {UserId} for vendor {VendorId}. Denying request.", userId, vendorId);
                return new NotFoundResult();
            }

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "vendors/{vendorId}")] UpdateVendorDetailsApiModel vendorDetailsApiModel,
            HttpRequest req,
            string vendorId,
            [Table("Vendors", "Vendors", "{vendorId}", Connection= "SosCafeStorage")] VendorDetailsEntity vendorDetailsEntity,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            var userId = req.Headers["X-MS-CLIENT-PRINCIPAL-ID"];
            log.LogInformation("Received PUT vendors request for vendor {VendorId} from user {UserId}.", vendorId, userId);

            // Authorise the request.
            var isAuthorised = await UserManagement.IsUserAuthorisedForVendor(vendorUserAssignmentsTable, userId, vendorId);
            if (!isAuthorised)
            {
                log.LogInformation("Received unauthorised request from user {UserId} for vendor {VendorId}. Denying request.", userId, vendorId);
                return new NotFoundResult();
            }

            // Perform validation on the properties.
            if (vendorDetailsApiModel.DateAcceptedTerms == null)
            {
                return new BadRequestErrorMessageResult("The terms must be accepted in order to update the vendor.");
            }
            else if (vendorDetailsApiModel.DateAcceptedTerms >= DateTime.Now)
            {
                return new BadRequestErrorMessageResult("The terms must be accepted with a valid date in order to update the vendor.");
            }

            // Update entity.
            vendorDetailsEntity.IsValidated = true;
            vendorDetailsEntity.DateAcceptedTerms = vendorDetailsApiModel.DateAcceptedTerms;

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
    }
}
