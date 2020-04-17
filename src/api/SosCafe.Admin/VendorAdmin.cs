using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SosCafe.Admin.Csv;
using SosCafe.Admin.Entities;
using SosCafe.Admin.Models.Api;

namespace SosCafe.Admin
{
    public static class VendorAdmin
    {
        [FunctionName("AdminGetVendor")]
        public static IActionResult AdminGetVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "internal/vendors/{vendorId}")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("Vendors", "Vendors", "{vendorId}", Connection = "SosCafeStorage")] VendorDetailsEntity vendorDetailsEntity,
            ILogger log)
        {
            // Check the authorisation.
            var isAuthorised = UserManagement.IsUserAuthorisedForAdmin(claimsPrincipal);
            if (!isAuthorised)
            {
                log.LogInformation("Received unauthorised admin request. Denying request.");
                return new NotFoundResult();
            }

            // Read the vendor details from table storage.
            if (vendorDetailsEntity == null)
            {
                log.LogWarning("Could not find vendor {VendorId}.", vendorId);
                return new NotFoundResult();
            }

            // Map to an API response.
            var vendorDetailsResponse = new InternalVendorDetailsApiModel
            {
                Id = vendorDetailsEntity.ShopifyId,
                RegisteredDate = vendorDetailsEntity.RegisteredDate,
                BusinessName = vendorDetailsEntity.BusinessName,
                ContactName = vendorDetailsEntity.ContactName,
                EmailAddress = vendorDetailsEntity.EmailAddress,
                PhoneNumber = vendorDetailsEntity.PhoneNumber,
                BankAccountNumber = vendorDetailsEntity.BankAccountNumber,
                HasAgreedToTerms = (vendorDetailsEntity.DateAcceptedTerms != null),
                InternalTag = vendorDetailsEntity.InternalTag
            };

            // Return the vendor details.
            return new OkObjectResult(vendorDetailsResponse);
        }

        [FunctionName("AdminSearchVendors")]
        public static async Task<IActionResult> AdminSearchVendors(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "internal/vendors")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorsTable,
            ILogger log)
        {
            // Check the authorisation.
            var isAuthorised = UserManagement.IsUserAuthorisedForAdmin(claimsPrincipal);
            if (!isAuthorised)
            {
                log.LogInformation("Received unauthorised admin request. Denying request.");
                return new NotFoundResult();
            }

            // Assemble the query filters.
            var filters = new List<string>();

            string vendorId = req.Query["vendorId"];
            if (!string.IsNullOrEmpty(vendorId))
            {
                filters.Add(TableQuery.GenerateFilterCondition(nameof(VendorDetailsEntity.RowKey), QueryComparisons.Equal, vendorId));
            }

            string name = req.Query["name"];
            if (!string.IsNullOrEmpty(name))
            {
                filters.Add(TableQuery.GenerateFilterCondition(nameof(VendorDetailsEntity.BusinessName), QueryComparisons.Equal, name));
            }

            string emailAddress = req.Query["emailAddress"];
            if (!string.IsNullOrEmpty(emailAddress))
            {
                filters.Add(TableQuery.GenerateFilterCondition(nameof(VendorDetailsEntity.EmailAddress), QueryComparisons.Equal, emailAddress));
            }

            string tag = req.Query["tag"];
            if (!string.IsNullOrEmpty(tag))
            {
                filters.Add(TableQuery.GenerateFilterCondition(nameof(VendorDetailsEntity.InternalTag), QueryComparisons.Equal, tag));
            }

            var filter = CombineTableFilters(filters);

            // Execute the query.
            TableContinuationToken token = null;
            var searchResults = new List<VendorDetailsEntity>();
            do
            {
                var queryResult = await vendorsTable.ExecuteQuerySegmentedAsync(new TableQuery<VendorDetailsEntity>().Where(filter), token);
                searchResults.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            // Map the results to a response model.
            var mappedResults = searchResults.Select(entity => new VendorSummaryApiModel
            {
                Id = entity.ShopifyId,
                BusinessName = entity.BusinessName
            });

            // Return the results.
            return new OkObjectResult(mappedResults);
        }

        [FunctionName("AdminExportVendorList")]
        public static async Task<IActionResult> AdminExportVendorList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "internal/vendors/csv")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            ILogger log)
        {
            // Check the authorisation.
            var isAuthorised = UserManagement.IsUserAuthorisedForAdmin(claimsPrincipal);
            if (!isAuthorised)
            {
                log.LogInformation("Received unauthorised admin request. Denying request.");
                return new NotFoundResult();
            }

            // Read all records from table storage.
            TableContinuationToken token = null;
            var allVendorDetails = new List<VendorDetailsEntity>();
            do
            {
                var queryResult = await vendorDetailsTable.ExecuteQuerySegmentedAsync(new TableQuery<VendorDetailsEntity>(), token);
                allVendorDetails.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            // Transform into VendorDetailsCsv objects so that we can roundtrip successfully.
            var allVendorDetailsCsv = allVendorDetails.Select(entity => new VendorDetailsCsv
            {
                ShopifyId = entity.ShopifyId,
                BusinessName = entity.BusinessName,
                RegisteredDate = entity.RegisteredDate,
                ContactName = entity.ContactName,
                PhoneNumber = entity.PhoneNumber,
                EmailAddress = entity.EmailAddress,
                BankAccountNumber = entity.BankAccountNumber,
                InternalTag = entity.InternalTag
            });

            // Serialize to CSV.
            var fileBytes = CsvCreator.CreateCsvFile(allVendorDetailsCsv);
            return new FileContentResult(fileBytes, "text/csv")
            {
                FileDownloadName = "SOSCafe-AllVendors.csv"
            };
        }

        private static string CombineTableFilters(List<string> filters)
        {
            string myQuery = string.Empty;
            var isFirst = true;

            foreach (string filter in filters)
            {
                if (isFirst)
                {
                    myQuery = filter;
                }
                else
                {
                    myQuery = TableQuery.CombineFilters(myQuery, TableOperators.And, filter);
                }
            }

            return myQuery;
        }
    }
}
