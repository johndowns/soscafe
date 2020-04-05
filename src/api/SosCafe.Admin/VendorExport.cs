using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System.Globalization;
using CsvHelper;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Http.Headers;

namespace SosCafe.Admin
{
    public static class VendorExport
    {
        [FunctionName("ExportVendorList")]
        public static async Task<HttpResponseMessage> ExportVendorList(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
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

            // Serialize to CSV.
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(allVendorDetails);

                // Return the CSV to the response stream.
                var stringToReturn = writer.ToString();
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(stringToReturn, Encoding.UTF8, "text/csv")
                };
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"AllVendors-{DateTime.Now:yyyy-MM-dd-HH-mm}.csv"
                };
                return response;
            }            
        }
    }
}
