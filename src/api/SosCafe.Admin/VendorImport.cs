using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using SosCafe.Admin.Csv;
using SosCafe.Admin.Entities;
using System.Collections.Generic;
using System;

namespace SosCafe.Admin.Models
{
    public static class VendorImport
    {
        [FunctionName("VendorListImport")]
        public static void VendorListImport(
            [BlobTrigger("vendorlistimport/{name}", Connection="SosCafeStorage")] Stream myBlob, string name,
            [Queue("importvendor"), StorageAccount("SosCafeStorage")] ICollector<VendorDetailsCsv> outputQueueMessages,
            ILogger log)
        {
            // Read the blob contents (in CSV format), shred into strongly typed model objects,
            // and add to a queue for processing.
            log.LogInformation("Processing file {FileName}, length {FileLength}.", name, myBlob.Length);

            var records = GetRecordsFromCsv<VendorDetailsCsv>(myBlob);
            log.LogInformation("Found {RecordCount} records.", records.Count);

            records.ForEach(vvCsv => outputQueueMessages.Add(vvCsv));
        }

        [FunctionName("ProcessImportedVendor")]
        public static async Task ProcessImportedVendor(
            [QueueTrigger("importvendor", Connection="SosCafeStorage")] VendorDetailsCsv vendorToImport,
            [Table("Vendors", Connection = "SosCafeStorage")] CloudTable vendorDetailsTable,
            [Table("VendorUserAssignments", Connection = "SosCafeStorage")] CloudTable vendorUserAssignmentsTable,
            ILogger log)
        {
            log.LogInformation("Processing vendor ID {VendorShopifyId}.", vendorToImport.ShopifyId);

            // Validate the vendor ID, as Excel can sometimes screw around with it.
            if (!long.TryParse(vendorToImport.ShopifyId, out _))
            {
                log.LogError("Vendor ID {VendorShopifyId} is invalid.", vendorToImport.ShopifyId);
                throw new ArgumentException($"Invalid vendor ID {vendorToImport.ShopifyId}");
            }

            // Convert the data to the entity format.
            var vendorEntity = new VendorDetailsEntity
            {
                ShopifyId = vendorToImport.ShopifyId,
                RegisteredDate = vendorToImport.RegisteredDate,
                BusinessName = vendorToImport.BusinessName,
                ContactName = vendorToImport.ContactName,
                EmailAddress = vendorToImport.EmailAddress.Trim(),
                PhoneNumber = vendorToImport.PhoneNumber,
                BankAccountNumber = vendorToImport.BankAccountNumber,
                DateAcceptedTerms = vendorToImport.DateAcceptedTerms?.ToString("o"),
                IsClickAndCollect = vendorToImport.IsClickAndCollect,
                Level1Closed = vendorToImport.Level1Closed,
                Level1Delivery = vendorToImport.Level1Delivery,
                Level1ClickAndCollect = vendorToImport.Level1ClickAndCollect,
                Level1Open = vendorToImport.Level1Open,
                Level2Closed = vendorToImport.Level2Closed,
                Level2Delivery = vendorToImport.Level2Delivery,
                Level2ClickAndCollect = vendorToImport.Level2ClickAndCollect,
                Level2Open = vendorToImport.Level2Open,
                Level3Closed = vendorToImport.Level3Closed,
                Level3Delivery = vendorToImport.Level3Delivery,
                Level3ClickAndCollect = vendorToImport.Level3ClickAndCollect,
                Level3Open = vendorToImport.Level3Open,
                ClickAndCollectUrl = vendorToImport.ClickAndCollectUrl
            };

            // Upsert vendor table entity.
            var upsertVendorDetailsEntityOperation = TableOperation.InsertOrReplace(vendorEntity);
            var upsertVendorDetailsEntityOperationResult = await vendorDetailsTable.ExecuteAsync(upsertVendorDetailsEntityOperation);
            if (upsertVendorDetailsEntityOperationResult.HttpStatusCode < 200 || upsertVendorDetailsEntityOperationResult.HttpStatusCode > 299)
            {
                log.LogError("Failed to upsert entity into Vendors table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertVendorDetailsEntityOperationResult.HttpStatusCode, upsertVendorDetailsEntityOperationResult.Result);
            }
            else
            {
                log.LogInformation("Upserted entity into Vendors table.");
            }

            // Create vendor role assignment for this user.
            var vendorUserAssignmentEntity = new VendorUserAssignmentEntity
            {
                VendorShopifyId = vendorToImport.ShopifyId,
                VendorName = vendorToImport.BusinessName,
                UserId = vendorToImport.EmailAddress.Trim()
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
        }

        [FunctionName("VendorPaymentsImport")]
        public static void VendorPaymentsImport(
            [BlobTrigger("vendorpaymentsimport/{name}", Connection = "SosCafeStorage")] Stream myBlob, string name,
            [Queue("importvendorpayment"), StorageAccount("SosCafeStorage")] ICollector<VendorPaymentCsv> outputQueueMessages,
            ILogger log)
        {
            // Read the blob contents (in CSV format), shred into strongly typed model objects,
            // and add to a queue for processing.
            log.LogInformation("Processing file {FileName}, length {FileLength}.", name, myBlob.Length);

            var records = GetRecordsFromCsv<VendorPaymentCsv>(myBlob);
            log.LogInformation("Found {RecordCount} records.", records.Count);

            records.ForEach(vvCsv => outputQueueMessages.Add(vvCsv));
        }

        [FunctionName("ProcessImportedVendorPayment")]
        public static async Task ProcessImportedVendorPayment(
            [QueueTrigger("importvendorpayment", Connection = "SosCafeStorage")] VendorPaymentCsv vendorPaymentToImport,
            [Table("VendorPayments", Connection = "SosCafeStorage")] CloudTable vendorPaymentsTable,
            ILogger log)
        {
            log.LogInformation("Processing vendor payment with payment ID {PaymentId}.", vendorPaymentToImport.PaymentId);

            // Validate the vendor ID, as Excel can sometimes screw around with it.
            if (!long.TryParse(vendorPaymentToImport.VendorId, out _))
            {
                log.LogError("Vendor ID {VendorShopifyId} is invalid.", vendorPaymentToImport.VendorId);
                throw new ArgumentException($"Invalid vendor ID {vendorPaymentToImport.VendorId}");
            }

            // Special case handling.
            if (vendorPaymentToImport.NetPayment.Trim() == "$-")
            {
                vendorPaymentToImport.NetPayment = "0";
            }

            // Convert the data to the entity format.
            var vendorPaymentEntity = new VendorPaymentEntity
            {
                VendorId = vendorPaymentToImport.VendorId,
                PaymentId = vendorPaymentToImport.PaymentId,
                PaymentDate = vendorPaymentToImport.PaymentDate,
                BankAccountNumber = vendorPaymentToImport.BankAccountNumber,
                NetPayment = decimal.Parse(vendorPaymentToImport.NetPayment, NumberStyles.Currency)
            };

            // Upsert vendor payment table entity.
            var upsertVendorPaymentEntityOperation = TableOperation.InsertOrReplace(vendorPaymentEntity);
            var upsertVendorPaymentEntityOperationResult = await vendorPaymentsTable.ExecuteAsync(upsertVendorPaymentEntityOperation);
            if (upsertVendorPaymentEntityOperationResult.HttpStatusCode < 200 || upsertVendorPaymentEntityOperationResult.HttpStatusCode > 299)
            {
                log.LogError("Failed to upsert entity into VendorPayments table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertVendorPaymentEntityOperationResult.HttpStatusCode, upsertVendorPaymentEntityOperationResult.Result);
            }
            else
            {
                log.LogInformation("Upserted entity into VendorPayments table.");
            }
        }

        private static List<T> GetRecordsFromCsv<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, new CultureInfo("en-NZ")))
            {
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.ShouldSkipRecord = record => record.All(string.IsNullOrEmpty);

                var records = csv.GetRecords<T>().ToList();

                return records;
            }
        }
    }
}
