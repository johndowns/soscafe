using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SosCafe.Admin.Csv;
using SosCafe.Admin.Entities;
using SosCafe.Admin.Models.Api;
using System.Text.RegularExpressions;
using PhoneNumbers;

namespace SosCafe.Admin
{
    public static class VendorAdmin
    {
        private static readonly PhoneNumberUtil PhoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();

        [FunctionName("AdminGetVendor")]
        public static IActionResult AdminGetVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "internal/vendors/{vendorId}")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("Vendors", "Vendors", "{vendorId}", Connection = "SosCafeStorage")] VendorDetailsEntity vendorDetailsEntity,
            [Table("HiddenVendors", "Vendors", "{vendorId}", Connection = "SosCafeStorage")] HiddenVendorEntity hiddenVendorEntity,
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
                IsHidden = (hiddenVendorEntity != null),
                RegisteredDate = vendorDetailsEntity.RegisteredDate,
                BusinessName = vendorDetailsEntity.BusinessName,
                ContactName = vendorDetailsEntity.ContactName,
                EmailAddress = vendorDetailsEntity.EmailAddress,
                PhoneNumber = vendorDetailsEntity.PhoneNumber,
                BankAccountNumber = vendorDetailsEntity.BankAccountNumber,
                HasAgreedToTerms = (vendorDetailsEntity.DateAcceptedTerms != null),
                IsClickAndCollect = vendorDetailsEntity.IsClickAndCollect,
                InternalTag = vendorDetailsEntity.InternalTag,
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

        [FunctionName("AdminUpdateVendor")]
        public static async Task<IActionResult> AdminUpdateVendor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "internal/vendors/{vendorId}")] InternalUpdateVendorDetailsApiModel vendorDetailsApiModel,
            HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            string vendorId,
            [Table("Vendors", "Vendors", "{vendorId}", Connection= "SosCafeStorage")] VendorDetailsEntity vendorDetailsEntity,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            [Table("HiddenVendors", Connection = "SosCafeStorage")] CloudTable hiddenVendorsTable,
            [Table("HiddenVendors", "Vendors", "{vendorId}", Connection = "SosCafeStorage")] HiddenVendorEntity hiddenVendorEntity,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            // Check the authorisation.
            var isAuthorised = UserManagement.IsUserAuthorisedForAdmin(claimsPrincipal);
            if (!isAuthorised)
            {
                log.LogInformation("Received unauthorised admin request. Denying request.");
                return new NotFoundResult();
            }

            // Perform validation on the properties.
            if (!VendorManagement.BankAccountRegex.IsMatch(vendorDetailsApiModel.BankAccountNumber))
            {
                return new BadRequestErrorMessageResult("The bank account number is invalid.");
            }

            // Handle hidden vendors.
            if (vendorDetailsApiModel.IsHidden && hiddenVendorEntity == null)
            {
                hiddenVendorEntity = new HiddenVendorEntity
                {
                    VendorShopifyId = vendorId
                };

                // Create or update the hidden vendor.
                var upsertHiddenVendorEntityOperation = TableOperation.InsertOrReplace(hiddenVendorEntity);
                var upsertHiddenVendorEntityOperationResult = await hiddenVendorsTable.ExecuteAsync(upsertHiddenVendorEntityOperation);
                if (upsertHiddenVendorEntityOperationResult.HttpStatusCode < 200 || upsertHiddenVendorEntityOperationResult.HttpStatusCode > 299)
                {
                    log.LogError("Failed to upsert entity into HiddenVendors table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertHiddenVendorEntityOperationResult.HttpStatusCode, upsertHiddenVendorEntityOperationResult.Result);
                }
                else
                {
                    log.LogInformation("Upserted entity into HiddenVendors table.");
                }
            }
            else if (! vendorDetailsApiModel.IsHidden && hiddenVendorEntity != null)
            {
                // Note the ETag on the entity, which is required to delete the entity from the table.
                hiddenVendorEntity.ETag = "*";

                // Delete the hidden vendor.
                var deleteHiddenVendorEntityOperation = TableOperation.Delete(hiddenVendorEntity);
                var deleteHiddenVendorEntityOperationResult = await hiddenVendorsTable.ExecuteAsync(deleteHiddenVendorEntityOperation);
                if (deleteHiddenVendorEntityOperationResult.HttpStatusCode == 404)
                {
                    log.LogInformation("Did not found a HiddenVendors entity to delete.");
                }
                else if (deleteHiddenVendorEntityOperationResult.HttpStatusCode < 200 || deleteHiddenVendorEntityOperationResult.HttpStatusCode > 299)
                {
                    log.LogError("Failed to delete entity from HiddenVendors table. Status code={UpsertStatusCode}, Result={DeleteResult}", deleteHiddenVendorEntityOperationResult.HttpStatusCode, deleteHiddenVendorEntityOperationResult.Result);
                }
                else
                {
                    log.LogInformation("Deleted entity from HiddenVendors table.");
                }
            }

            // Detect if email address has changed.
            if (vendorDetailsApiModel.EmailAddress != vendorDetailsEntity.EmailAddress)
            {
                // Create vendor role assignment for this user.
                var vendorUserAssignmentEntity = new VendorUserAssignmentEntity
                {
                    VendorShopifyId = vendorId,
                    VendorName = vendorDetailsApiModel.BusinessName,
                    UserId = vendorDetailsApiModel.EmailAddress.Trim()
                };

                // Upsert vendor user assignment entity.
                var upsertVendorUserAssignmentEntityOperation = TableOperation.InsertOrReplace(vendorUserAssignmentEntity);
                var upsertVendorUserAssignmentEntityOperationResult = await vendorUserAssignmentsTable.ExecuteAsync(upsertVendorUserAssignmentEntityOperation);
                if (upsertVendorUserAssignmentEntityOperationResult.HttpStatusCode < 200 || upsertVendorUserAssignmentEntityOperationResult.HttpStatusCode > 299)
                {
                    log.LogError("Failed to upsert entity into VendorUserAssignments table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertVendorUserAssignmentEntityOperationResult.HttpStatusCode, upsertVendorUserAssignmentEntityOperationResult.Result);
                }
                else
                {
                    log.LogInformation("Upserted entity into VendorUserAssignments table.");
                }

                // Update vendor entity with new email address.
                vendorDetailsEntity.EmailAddress = vendorDetailsApiModel.EmailAddress;
            }

            // Detect if the business name has changed.
            if (vendorDetailsApiModel.BusinessName != vendorDetailsEntity.BusinessName)
            {
                await UpdateVendorUserAssignmentBusinessNames(vendorUserAssignmentsTable, vendorId, vendorDetailsApiModel.BusinessName, log);

                // Update vendor entity with new business name.
                vendorDetailsEntity.BusinessName = vendorDetailsApiModel.BusinessName;
            }

            // Update entity.
            vendorDetailsEntity.ContactName = vendorDetailsApiModel.ContactName;
            vendorDetailsEntity.RegisteredDate = vendorDetailsApiModel.RegisteredDate;
            vendorDetailsEntity.PhoneNumber = vendorDetailsApiModel.PhoneNumber;
            vendorDetailsEntity.BankAccountNumber = vendorDetailsApiModel.BankAccountNumber;
            vendorDetailsEntity.IsClickAndCollect = vendorDetailsApiModel.IsClickAndCollect;
            vendorDetailsEntity.InternalTag = vendorDetailsApiModel.InternalTag;
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

            // Perform the search.
            var allVendorEntities = new List<VendorDetailsEntity>();

            string vendorId = req.Query["vendorId"];
            if (!string.IsNullOrEmpty(vendorId))
            {
                // If a vendor ID is specified, treat this as a point read and don't do any further searching.
                // Read the vendor details from table storage.
                var findOperation = TableOperation.Retrieve<VendorDetailsEntity>("Vendors", vendorId);
                var findResult = await vendorsTable.ExecuteAsync(findOperation);
                if (findResult.Result != null)
                {
                    allVendorEntities.Add((VendorDetailsEntity)findResult.Result);
                }
            }
            else
            {
                // Otherwise, pull down the whole set of vendors and do an in-memory search.
                allVendorEntities = await GetAllVendors(vendorsTable);

                string name = req.Query["name"];
                if (!string.IsNullOrEmpty(name))
                {
                    allVendorEntities = allVendorEntities.Where(v => v.BusinessName.Contains(name, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
                }

                string emailAddress = req.Query["emailAddress"];
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    allVendorEntities = allVendorEntities.Where(v => v.EmailAddress.Contains(emailAddress, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
                }

                string tag = req.Query["tag"];
                if (!string.IsNullOrEmpty(tag))
                {
                    allVendorEntities = allVendorEntities.Where(v => v.InternalTag != null && v.InternalTag.Contains(tag, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
            }

            // Map the results to a response model.
            var mappedResults = allVendorEntities.Select(entity => new VendorSummaryApiModel
            {
                Id = entity.ShopifyId,
                BusinessName = entity.BusinessName
            })
                .OrderBy(d => d.BusinessName);

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
            var allVendorEntities = await GetAllVendors(vendorDetailsTable);

            // Transform into VendorDetailsCsv objects so that we can roundtrip successfully.
            var allVendorDetailsCsv = allVendorEntities.Select(entity => new VendorDetailsCsv
            {
                ShopifyId = entity.ShopifyId,
                BusinessName = entity.BusinessName,
                RegisteredDate = entity.RegisteredDate,
                ContactName = entity.ContactName,
                PhoneNumber = FormatPhoneNumber(entity.PhoneNumber),
                EmailAddress = entity.EmailAddress,
                BankAccountNumber = FormatBankAccountNumber(entity.BankAccountNumber),
                IsClickAndCollect = entity.IsClickAndCollect,
                InternalTag = entity.InternalTag,
                Level1Closed = entity.Level1Closed,
                Level1Delivery = entity.Level1Delivery,
                Level1ClickAndCollect = entity.Level1ClickAndCollect,
                Level1Open = entity.Level1Open,
                Level2Closed = entity.Level2Closed,
                Level2Delivery = entity.Level2Delivery,
                Level2ClickAndCollect = entity.Level2ClickAndCollect,
                Level2Open = entity.Level2Open,
                Level3Closed = entity.Level3Closed,
                Level3Delivery = entity.Level3Delivery,
                Level3ClickAndCollect = entity.Level3ClickAndCollect,
                Level3Open = entity.Level3Open,
                ClickAndCollectUrl = entity.ClickAndCollectUrl
            })
                .OrderBy(d => d.BusinessName);

            // Serialize to CSV.
            var fileBytes = CsvCreator.CreateCsvFile(allVendorDetailsCsv);
            return new FileContentResult(fileBytes, "text/csv")
            {
                FileDownloadName = "SOSCafe-AllVendors.csv"
            };
        }

        [FunctionName("AdminExportVoucherList")]
        public static async Task<IActionResult> AdminExportVoucherList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "internal/vouchers/csv")] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            [Table("ShopifyVouchers", Connection = "SosCafeStorage")] CloudTable shopifyVouchersTable,
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
            var allVoucherEntities = await GetAllVouchers(shopifyVouchersTable);

            // Transform into VoucherDetailsCsv objects.
            var allVoucherDetailsCsv = allVoucherEntities.Select(entity => new AdminVendorVoucherCsv
            {
                VendorId = entity.VendorId,
                OrderId = entity.OrderId,
                OrderRef = entity.OrderRef,
                CustomerName = entity.CustomerName,
                CustomerEmailAddress = entity.CustomerEmailAddress,
                OrderDate = VendorManagement.GetNewZealandTimeFromUtc(entity.OrderDate),
                LineItemId = entity.LineItemId,
                VoucherId = VendorManagement.GetVoucherIdForDisplay(entity),
                PaymentGateway = entity.Gateway,
                VoucherQuantity = entity.VoucherQuantity,
                VoucherIsDonation = VendorManagement.IsVoucherDonation(entity).ToString(),
                VoucherGross = ((decimal)entity.VoucherGross *  (decimal)1.005).ToString(),
                VoucherFees = entity.VoucherFees.ToString(),
                VoucherNet = ((decimal)entity.VoucherNet * (decimal)1.005).ToString(),
                IsRefunded = entity.IsRefunded.ToString()
            })
                .OrderByDescending(d => d.OrderDate);

            // Serialize to CSV.
            var fileBytes = CsvCreator.CreateCsvFile(allVoucherDetailsCsv);
            return new FileContentResult(fileBytes, "text/csv")
            {
                FileDownloadName = "SOSCafe-AllVouchers.csv"
            };
        }

        private static async Task<List<VendorDetailsEntity>> GetAllVendors(CloudTable vendorsTable)
        {
            TableContinuationToken token = null;
            var allVendorDetails = new List<VendorDetailsEntity>();
            do
            {
                var queryResult = await vendorsTable.ExecuteQuerySegmentedAsync(new TableQuery<VendorDetailsEntity>(), token);
                allVendorDetails.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return allVendorDetails;
        }

        private static async Task<List<VendorVoucherEntity>> GetAllVouchers(CloudTable vouchersTable)
        {
            TableContinuationToken token = null;
            var allVoucherDetails = new List<VendorVoucherEntity>();
            do
            {
                var queryResult = await vouchersTable.ExecuteQuerySegmentedAsync(new TableQuery<VendorVoucherEntity>(), token);
                allVoucherDetails.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return allVoucherDetails;
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

        private static string FormatBankAccountNumber(string bankAccountNumber)
        {
            string bankAccountNumberWithNumbersOnly = string.Concat(bankAccountNumber.Where(char.IsDigit));
            if (bankAccountNumberWithNumbersOnly.Length < 15 || bankAccountNumberWithNumbersOnly.Length > 16) return bankAccountNumberWithNumbersOnly;
            return Regex.Replace(bankAccountNumberWithNumbersOnly, @"(\w{2})(\w{4})(\w{7})(\w{2,3})", @"$1-$2-$3-$4");
        }

        private static string FormatPhoneNumber(string phoneNumber)
        {
            try
            {
                var phoneNumberParsed = PhoneNumberUtil.Parse(phoneNumber.Trim(), "NZ");
                return PhoneNumberUtil.Format(phoneNumberParsed, PhoneNumberFormat.NATIONAL);
            }
            catch (NumberParseException)
            {
                return phoneNumber.Trim();
            }
        }

        private static async Task UpdateVendorUserAssignmentBusinessNames(CloudTable vendorUserAssignmentsTable, string vendorId, string businessName, ILogger log)
        {
            // Find all vendor user assignments for this vendor ID.
            TableContinuationToken token = null;
            var vendorUserAssignments = new List<VendorUserAssignmentEntity>();
            var filterToUserRows = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, vendorId);
            do
            {
                var queryResult = await vendorUserAssignmentsTable.ExecuteQuerySegmentedAsync(new TableQuery<VendorUserAssignmentEntity>().Where(filterToUserRows), token);
                vendorUserAssignments.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            // Update each assignment's business name.
            foreach (var vendorUserAssignment in vendorUserAssignments)
            {
                vendorUserAssignment.VendorName = businessName;

                var upsertVendorUserAssignmentEntityOperation = TableOperation.InsertOrReplace(vendorUserAssignment);
                var upsertVendorUserAssignmentEntityOperationResult = await vendorUserAssignmentsTable.ExecuteAsync(upsertVendorUserAssignmentEntityOperation);
                if (upsertVendorUserAssignmentEntityOperationResult.HttpStatusCode < 200 || upsertVendorUserAssignmentEntityOperationResult.HttpStatusCode > 299)
                {
                    log.LogError("Failed to upsert entity into VendorUserAssignments table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertVendorUserAssignmentEntityOperationResult.HttpStatusCode, upsertVendorUserAssignmentEntityOperationResult.Result);
                }
                else
                {
                    log.LogInformation("Upserted entity into VendorUserAssignments table.");
                }
            }
        }
    }
}
