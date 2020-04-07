using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SosCafe.Admin
{
    public class VendorVoucherEntity : TableEntity
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

        private string orderId;
        public string OrderId
        {
            get
            {
                return orderId;
            }
            set
            {
                orderId = value;
                RowKey = OrderId;
            }
        }

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
    }
}
