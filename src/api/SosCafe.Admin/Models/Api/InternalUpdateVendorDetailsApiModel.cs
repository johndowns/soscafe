namespace SosCafe.Admin.Models.Api
{
    public class InternalUpdateVendorDetailsApiModel : UpdateVendorDetailsApiModel
    {
        public string InternalTag { get; set; }

        public string BankAccountNumber { get; set; }

        public bool IsHidden { get; set; }
    }
}
