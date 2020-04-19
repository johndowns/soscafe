using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SosCafe.Admin.Models.Api;
using SosCafe.Admin.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SosCafe.Admin
{
    public static class UserManagement
    {
        [FunctionName("GetVendorsForUser")]
        public static async Task<IActionResult> GetVendorsForUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendors")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Get the user principal ID.
            var userId = GetUserId(claimsPrincipal);

            // Read all records from table storage where the partition key is the user's ID.
            TableContinuationToken token = null;
            var availableVendorAssignments = new List<VendorUserAssignmentEntity>();
            var filterToUserPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId.CleanStringForPartitionKey().ToUpper());
            do
            {
                var queryResult = await vendorUserAssignmentsTable.ExecuteQuerySegmentedAsync(new TableQuery<VendorUserAssignmentEntity>().Where(filterToUserPartition), token);
                availableVendorAssignments.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            // Map the results to a response model.
            var mappedResults = availableVendorAssignments.Select(entity => new VendorSummaryApiModel
            {
                Id = entity.VendorShopifyId,
                BusinessName = entity.VendorName
            })
                .OrderBy(d => d.BusinessName);

            // Return the results.
            return new OkObjectResult(mappedResults);
        }

        internal static string GetUserId(ClaimsPrincipal claimsPrincipal)
        {
            return GetEmailAddress(claimsPrincipal).ToUpper();
        }

        internal static string GetEmailAddress(ClaimsPrincipal claimsPrincipal)
        {
            var userEmailAddress = (claimsPrincipal.Identity as ClaimsIdentity).Claims.FirstOrDefault(c => c.Type == "emails")?.Value;
            return userEmailAddress ?? string.Empty;
        }

        internal static string GetDisplayName(ClaimsPrincipal claimsPrincipal)
        {
            var userDisplayName = (claimsPrincipal.Identity as ClaimsIdentity).Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            return userDisplayName ?? string.Empty;
        }

        internal static bool GetIsAdminClaim(ClaimsPrincipal claimsPrincipal)
        {
            var isAdminClaim = (claimsPrincipal.Identity as ClaimsIdentity).Claims.FirstOrDefault(c => c.Type == "isadmin")?.Value;
            return !string.IsNullOrEmpty(isAdminClaim);
        }

        internal static async Task<bool> IsUserAuthorisedForVendor(CloudTable vendorUserAssignmentsTable, ClaimsPrincipal claimsPrincipal, string vendorId)
        {
            // Get the user ID.
            var userId = GetUserId(claimsPrincipal);

            // If the user is an admin user, they are automatically authorised.
            if (GetIsAdminClaim(claimsPrincipal))
            {
                return true;
            }

            // Otherwise, check that the user-vendor combination exists.
            var cleanedUserId = userId.CleanStringForPartitionKey().ToUpper();
            var findOperation = TableOperation.Retrieve<VendorDetailsEntity>(cleanedUserId, vendorId);
            var findResult = await vendorUserAssignmentsTable.ExecuteAsync(findOperation);
            return findResult.Result != null;
        }

        internal static bool IsUserAuthorisedForAdmin(ClaimsPrincipal claimsPrincipal)
        {
            return GetIsAdminClaim(claimsPrincipal);
        }
    }
}
