using System;
using CsvHelper.Configuration.Attributes;

namespace SosCafe.Admin.Csv
{
    public class VendorDetailsCsv
    {
        [Name("Registered")]
        [Index(0)]
        [Format("o")]
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
        [Format("o")]
        public DateTime? DateAcceptedTerms { get; set; }

        [Name("Click and Collect")]
        [Index(7)]
        public bool IsClickAndCollect { get; set; }

        [Name("InternalTag")]
        [Index(9)]
        public string InternalTag { get; set; }

        [Name("Level1Closed")]
        [Index(10)]
        public bool Level1Closed { get; set; }

        [Name("Level1Delivery")]
        [Index(11)]
        public bool Level1Delivery { get; set; }

        [Name("Level1ClickAndCollect")]
        [Index(12)]
        public bool Level1ClickAndCollect { get; set; }

        [Name("Level1Open")]
        [Index(13)]
        public bool Level1Open { get; set; }

        [Name("Level2Closed")]
        [Index(14)]
        public bool Level2Closed { get; set; }

        [Name("Level2Delivery")]
        [Index(15)]
        public bool Level2Delivery { get; set; }

        [Name("Level2ClickAndCollect")]
        [Index(16)]
        public bool Level2ClickAndCollect { get; set; }

        [Name("Level2Open")]
        [Index(17)]
        public bool Level2Open { get; set; }

        [Name("Level3Closed")]
        [Index(18)]
        public bool Level3Closed { get; set; }

        [Name("Level3Delivery")]
        [Index(19)]
        public bool Level3Delivery { get; set; }

        [Name("Level3ClickAndCollect")]
        [Index(20)]
        public bool Level3ClickAndCollect { get; set; }

        [Name("Level3Open")]
        [Index(21)]
        public bool Level3Open { get; set; }

        [Name("ClickAndCollectUrl")]
        [Index(22)]
        public string ClickAndCollectUrl { get; set; }

    }
}
