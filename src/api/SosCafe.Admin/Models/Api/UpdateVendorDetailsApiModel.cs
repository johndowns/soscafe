using System;

namespace SosCafe.Admin.Models.Api
{
    public class UpdateVendorDetailsApiModel
    {
        public string Id { get; set; }

        public string BusinessName { get; set; }

        public string ContactName { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string BankAccountNumber { get; set; }

        public DateTime? DateAcceptedTerms { get; set; }

        public bool IsClickAndCollect { get; set; }
    }
}
