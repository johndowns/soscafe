using System;

namespace SosCafe.Admin.Models.Api
{
    public class VendorVoucherApiModel
    {
        public string LineItemId { get; set; }

        public string OrderId { get; set; }

        public string OrderRef { get; set; }

        public DateTime OrderDate { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmailAddress { get; set; }

        public string CustomerRegion { get; set; }

        public bool CustomerAcceptsMarketing { get; set; }

        public string VoucherDescription { get; set; }

        public int VoucherQuantity { get; set; }

        public bool VoucherIsDonation { get; set; }

        public string VoucherId { get; set; }

        public decimal VoucherGross { get; set; }

        public decimal VoucherFees { get; set; }

        public decimal VoucherNet { get; set; }

        public DateTime? RedemptionDate { get; set; }
    }
}
