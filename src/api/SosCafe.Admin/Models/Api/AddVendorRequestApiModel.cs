using System;
using System.Collections.Generic;
using System.Text;

namespace SosCafe.Admin.Models.Api
{
    public class AddVendorRequestApiModel
    {
        public string BusinessName { get; set; }
        public string Type { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string BusinessPhotoUrl { get; set; }
        public string BankAccountNumber { get; set; }
        public DateTime DateAcceptedTerms { get; set; }
        public bool IsClickAndCollect { get; set; }
    }
}
