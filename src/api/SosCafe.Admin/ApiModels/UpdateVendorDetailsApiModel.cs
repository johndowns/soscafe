using System;

namespace SosCafe.Admin.ApiModels
{
    public class UpdateVendorDetailsApiModel : VendorDetailsApiModel
    {
        public DateTime? DateAcceptedTerms { get; set; }
    }
}
