using System;

namespace SosCafe.Admin.Entities
{
    public class VendorVoucherRedemptionEntity : SosCafeEntity
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

        public DateTime RedemptionDate { get; set; }
    }
}
