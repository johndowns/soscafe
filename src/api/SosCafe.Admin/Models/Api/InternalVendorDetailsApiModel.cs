namespace SosCafe.Admin.Models.Api
{
    public class InternalVendorDetailsApiModel : VendorDetailsApiModel
    {
        public bool IsHidden { get; set; }

        public string InternalTag { get; set; }
    }
}
