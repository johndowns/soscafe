using System;

namespace SosCafe.Admin.Models.Api
{
    public class UpdateVendorDetailsApiModel : VendorDetailsApiModel
    {
        public DateTime? DateAcceptedTerms { get; set; }
    }
}
