using System;
using CsvHelper.Configuration.Attributes;

namespace SosCafe.Admin.Csv
{
    public class VendorDetailsCsv
    {
        [Name("Registered")]
        [Index(0)]
        public DateTime RegisteredDate { get; set; }

        [Name("Business Name")]
        [Index(2)]
        public string BusinessName { get; set; }

        [Name("Shopify ID")]
        [Index(1)]
        public string ShopifyId{ get; set; }

        [Name("Contact Name")]
        [Index(3)]
        public string ContactName { get; set; }

        [Name("Email")]
        [Index(4)]
        public string EmailAddress { get; set; }

        [Name("Phone Number")]
        [Index(5)]
        public string PhoneNumber { get; set; }

        [Name("Bank Account")]
        [Index(6)]
        public string BankAccountNumber { get; set; }

        [Name("Date Accepted Terms")]
        [Index(8)]
        public DateTime? DateAcceptedTerms { get; set; }

        [Name("Click and Collect")]
        [Index(7)]
        public bool IsClickAndCollect { get; set; }

        [Name("InternalTag")]
        [Index(9)]
        public string InternalTag { get; set; }
    }
}
