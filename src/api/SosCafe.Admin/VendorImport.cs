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
                DateAcceptedTerms = vendorToImport.DateAcceptedTerms?.ToString("o")
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
            if (upsertVendorUserAssignmentEntityOperationResult.HttpStatusCode < 200 || upsertVendorDetailsEntityOperationResult.HttpStatusCode > 299)
            {
                log.LogError("Failed to upsert entity into VendorUserAssignments table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertVendorDetailsEntityOperationResult.HttpStatusCode, upsertVendorDetailsEntityOperationResult.Result);
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

        [FunctionName("VendorVouchersImport")]
        public static void VendorVouchersImport(
            [BlobTrigger("vendorvouchersimport/{name}", Connection = "SosCafeStorage")] Stream myBlob, string name,
            [Queue("importvendorvoucher"), StorageAccount("SosCafeStorage")] ICollector<VendorVoucherCsv> outputQueueMessages,
            ILogger log)
        {
            // Read the blob contents (in CSV format), shred into strongly typed model objects,
            // and add to a queue for processing.
            log.LogInformation("Processing file {FileName}, length {FileLength}.", name, myBlob.Length);

            var records = GetRecordsFromCsv<VendorVoucherCsv>(myBlob);
            log.LogInformation("Found {RecordCount} records.", records.Count);

            records.ForEach(vvCsv => outputQueueMessages.Add(vvCsv));

        }

        [FunctionName("ProcessImportedVendorVoucher")]
        public static async Task ProcessImportedVendorVoucher(
            [QueueTrigger("importvendorvoucher", Connection = "SosCafeStorage")] VendorVoucherCsv vendorVoucherToImport,
            [Table("VendorVouchers", Connection = "SosCafeStorage")] CloudTable vendorVouchersTable,
            ILogger log)
        {
            log.LogInformation("Processing vendor voucher with order ID {OrderId}.", vendorVoucherToImport.OrderId);

            // Special case handling.
            if (vendorVoucherToImport.VoucherGross.Trim() == "$-")
            {
                vendorVoucherToImport.VoucherGross = "0";
            }
            if (vendorVoucherToImport.VoucherFees.Trim() == "$-")
            {
                vendorVoucherToImport.VoucherFees = "0";
            }
            if (vendorVoucherToImport.VoucherNet.Trim() == "$-")
            {
                vendorVoucherToImport.VoucherNet = "0";
            }

            // Convert the data to the entity format.
            var vendorVoucherEntity = new VendorVoucherEntity
            {
                VendorId = vendorVoucherToImport.VendorId,
                LineItemId = vendorVoucherToImport.LineItemId,
                OrderId = vendorVoucherToImport.OrderId,
                OrderRef = vendorVoucherToImport.OrderRef,
                OrderDate = vendorVoucherToImport.OrderDate,
                CustomerName = vendorVoucherToImport.CustomerName,
                CustomerEmailAddress = vendorVoucherToImport.CustomerEmailAddress,
                CustomerRegion = vendorVoucherToImport.CustomerRegion,
                CustomerAcceptsMarketing = vendorVoucherToImport.CustomerAcceptsMarketing.Contains("TRUE"),
                VoucherDescription = vendorVoucherToImport.VoucherDescription,
                VoucherQuantity = vendorVoucherToImport.VoucherQuantity,
                VoucherIsDonation = vendorVoucherToImport.VoucherIsDonation.Contains("TRUE"),
                VoucherId = vendorVoucherToImport.VoucherId,
                VoucherGross = decimal.Parse(vendorVoucherToImport.VoucherGross, NumberStyles.Currency),
                VoucherFees = decimal.Parse(vendorVoucherToImport.VoucherFees, NumberStyles.Currency),
                VoucherNet = decimal.Parse(vendorVoucherToImport.VoucherNet, NumberStyles.Currency)
            };

            // Upsert vendor voucher table entity.
            var upsertVendorVoucherEntityOperation = TableOperation.InsertOrReplace(vendorVoucherEntity);
            var upsertVendorVoucherEntityOperationResult = await vendorVouchersTable.ExecuteAsync(upsertVendorVoucherEntityOperation);
            if (upsertVendorVoucherEntityOperationResult.HttpStatusCode < 200 || upsertVendorVoucherEntityOperationResult.HttpStatusCode > 299)
            {
                log.LogError("Failed to upsert entity into VendorVouchers table. Status code={UpsertStatusCode}, Result={InsertResult}", upsertVendorVoucherEntityOperationResult.HttpStatusCode, upsertVendorVoucherEntityOperationResult.Result);
            }
            else
            {
                log.LogInformation("Upserted entity into VendorVouchers table.");
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
