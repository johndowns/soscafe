using System;
using CsvHelper.Configuration.Attributes;

namespace SosCafe.Admin.Csv
{
    public class VendorDetailsCsv
    {
        [Name("Registered")]
        public DateTime RegisteredDate { get; set; }

        [Name("Business Name")]
        public string BusinessName { get; set; }

        [Name("Shopify ID")]
        public string ShopifyId{ get; set; }

        [Name("Contact Name")]
        public string ContactName { get; set; }

        [Name("Email")]
        public string EmailAddress { get; set; }

        [Name("Phone Number")]
        public string PhoneNumber { get; set; }

        [Name("Bank Account")]
        public string BankAccountNumber { get; set; }

        [Name("Date Accepted Terms")]
        public DateTime? DateAcceptedTerms { get; set; }

        [Name("InternalTag")]
        public string InternalTag { get; set; }
    }
}
