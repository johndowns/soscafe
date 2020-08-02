using System;

namespace SosCafe.Admin.Models.Api
{
    public class UpdateVendorDetailsApiModel
    {
        public string Id { get; set; }

        public string BusinessName { get; set; }

        public string ContactName { get; set; }

        public DateTime RegisteredDate { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime? DateAcceptedTerms { get; set; }

        public bool IsClickAndCollect { get; set; }

        public bool Level1Closed { get; set; }

        public bool Level1Delivery { get; set; }

        public bool Level1ClickAndCollect { get; set; }

        public bool Level1Open { get; set; }

        public bool Level2Closed { get; set; }

        public bool Level2Delivery { get; set; }

        public bool Level2ClickAndCollect { get; set; }

        public bool Level2Open { get; set; }

        public bool Level3Closed { get; set; }

        public bool Level3Delivery { get; set; }

        public bool Level3ClickAndCollect { get; set; }

        public bool Level3Open { get; set; }

        public string ClickAndCollectUrl { get; set; }
    }
}
