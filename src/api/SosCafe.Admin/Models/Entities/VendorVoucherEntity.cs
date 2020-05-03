using System;

namespace SosCafe.Admin.Entities
{
    public class VendorVoucherEntity : SosCafeEntity
    {
        private string vendorId;
        public string VendorId
        {
            get
            {
                return vendorId;
            }
            set
            {
                vendorId = value;
                PartitionKey = VendorId;
            }
        }

        private string lineItemId;
        public string LineItemId
        {
            get
            {
                return lineItemId;
            }
            set
            {
                lineItemId = value;
                RowKey = LineItemId;
            }
        }

        public string OrderId { get; set; }

        public string OrderRef { get; set; }

        public DateTime OrderDate { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmailAddress { get; set; }

        public string CustomerRegion { get; set; }

        public bool CustomerAcceptsMarketing { get; set; }

        public string VoucherDescription { get; set; }

        public int VoucherQuantity { get; set; }

        public string VoucherId { get; set; }

        public decimal VoucherGross { get; set; }

        public decimal VoucherFees { get; set; }

        public decimal VoucherNet { get; set; }

        public string VoucherType { get; set; }

        public bool IsRefunded { get; set; }

        public string Gateway { get; set; }
    }
}
