using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System.Globalization;
using CsvHelper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SosCafe.Admin.Entities;
using System.Linq;
using SosCafe.Admin.Csv;
using System.Text.RegularExpressions;

namespace SosCafe.Admin
{
    public static class VendorExport
    {
        [FunctionName("ExportVendorList")]
        public static async Task<IActionResult> ExportVendorList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [Blob("exports/AllVendors-{DateTime}.csv", FileAccess.Write, Connection = "SosCafeStorage")] Stream outputFile,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            ILogger log)
        {
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
                BankAccountNumber = FormatBankAccountNumber(entity.BankAccountNumber),
                IsClickAndCollect = entity.IsClickAndCollect
            });

            // Serialize to CSV.
            using (var writer = new StreamWriter(outputFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(allVendorDetailsCsv);
            }

            return new AcceptedResult();
        }

        private static string FormatBankAccountNumber(string bankAccountNumber)
        {
            string bankAccountNumberWithNumbersOnly = string.Concat(bankAccountNumber.Where(char.IsDigit));
            if (bankAccountNumberWithNumbersOnly.Length < 15 || bankAccountNumberWithNumbersOnly.Length > 16) return bankAccountNumberWithNumbersOnly;
            return Regex.Replace(bankAccountNumberWithNumbersOnly, @"(\w{2})(\w{4})(\w{7})(\w{2,3})", @"$1-$2-$3-$4");
        }
    }
}
