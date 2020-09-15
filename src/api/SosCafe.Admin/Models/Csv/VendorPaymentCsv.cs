using CsvHelper.Configuration.Attributes;
using System;

namespace SosCafe.Admin.Csv
{
    public class VendorPaymentExportCsv
    {
        [Name("Vendor ID")]
        public string VendorId { get; set; }

        [Name("Payment Reference")]
        public string PaymentId { get; set; }

        [Name("Date")]
        [Format("o")]
        public DateTime PaymentDate { get; set; }

        [Name("Bank account")]
        public string BankAccountNumber { get; set; }

        [Name("Net Payment")]
        public string NetPayment { get; set; }
    }

    public class VendorPaymentImportCsv
    {
        [Name("Vendor ID")]
        public string VendorId { get; set; }

        [Name("Payment Reference")]
        public string PaymentId { get; set; }

        [Name("Date")]
        public DateTime PaymentDate { get; set; }

        [Name("Bank account")]
        public string BankAccountNumber { get; set; }

        [Name("Net Payment")]
        public string NetPayment { get; set; }
    }
}
