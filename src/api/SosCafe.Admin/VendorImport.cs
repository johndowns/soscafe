using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace SosCafe.Admin.Models
{
    public static class VendorImport
    {
        [FunctionName("VendorListImport")]
        public static void VendorListImport(
            [BlobTrigger("vendorlistimport/{name}", Connection="SosCafeStorage")] Stream myBlob, string name,
            [Queue("importvendor"), StorageAccount("SosCafeStorage")] ICollector<VendorDetailsEntity> outputQueueMessages,
            ILogger log)
        {
            // Read the blob contents (in CSV format), shred into strongly typed model objects,
            // and add to a queue for processing.
            log.LogInformation("Processing file {FileName}, length {FileLength}.", name, myBlob.Length);

            using (var reader = new StreamReader(myBlob))
            using (var csv = new CsvReader(reader, new CultureInfo("en-NZ")))
            {
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;

                var records = csv.GetRecords<VendorDetailsCsv>().ToList();
                log.LogInformation("Found {RecordCount} records.", records.Count);

                // Add the record to the queue using the output binding.
                records.ForEach(vdCsv => outputQueueMessages.Add(new VendorDetailsEntity
                {
                    ShopifyId = vdCsv.ShopifyId,
                    RegisteredDate = vdCsv.RegisteredDate,
                    BusinessName = vdCsv.BusinessName,
                    ContactName = vdCsv.ContactName,
                    EmailAddress = vdCsv.EmailAddress,
                    PhoneNumber = vdCsv.PhoneNumber,
                    BankAccountNumber = vdCsv.BankAccountNumber,
                    IsValidated = false,
                    DateAcceptedTerms = null
                }));
           }
        }

        [FunctionName("ProcessImportedVendor")]
        public static async Task ProcessImportedVendor(
            [QueueTrigger("importvendor", Connection="SosCafeStorage")] VendorDetailsEntity vendorToImport,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            log.LogInformation("Processing vendor ID {VendorShopifyId}.", vendorToImport.ShopifyId);

            // Insert vendor table entity.
            var insertVendorDetailsEntityOperation = TableOperation.Insert(vendorToImport);
            var insertVendorDetailsEntityOperationResult = await vendorDetailsTable.ExecuteAsync(insertVendorDetailsEntityOperation);
            if (insertVendorDetailsEntityOperationResult.HttpStatusCode < 200 || insertVendorDetailsEntityOperationResult.HttpStatusCode > 299)
            {
                log.LogError("Failed to insert entity into Vendors table. Status code={InsertStatusCode}, Result={InsertResult}", insertVendorDetailsEntityOperationResult.HttpStatusCode, insertVendorDetailsEntityOperationResult.Result);
            }
            else
            {
                log.LogInformation("Inserted entity into Vendors table.");
            }

            // Create vendor role assignment for this user.
            var vendorUserAssignmentEntity = new VendorUserAssignmentEntity
            {
                VendorShopifyId = vendorToImport.ShopifyId,
                VendorName = vendorToImport.BusinessName,
                UserId = vendorToImport.EmailAddress
            };

            // Insert vendor user assignment entity.
            var insertVendorUserAssignmentEntityOperation = TableOperation.Insert(vendorUserAssignmentEntity);
            var insertVendorUserAssignmentEntityOperationResult = await vendorUserAssignmentsTable.ExecuteAsync(insertVendorUserAssignmentEntityOperation);
            if (insertVendorUserAssignmentEntityOperationResult.HttpStatusCode < 200 || insertVendorDetailsEntityOperationResult.HttpStatusCode > 299)
            {
                log.LogError("Failed to insert entity into VendorUserAssignments table. Status code={InsertStatusCode}, Result={InsertResult}", insertVendorDetailsEntityOperationResult.HttpStatusCode, insertVendorDetailsEntityOperationResult.Result);
            }
            else
            {
                log.LogInformation("Inserted entity into VendorUserAssignments table.");
            }
        }
    }
}
