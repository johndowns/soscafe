using Microsoft.WindowsAzure.Storage.Table;

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

        private string voucherId;
        public string VoucherId
        {
            get
            {
                return voucherId;
            }
            set
            {
                voucherId = value;
                RowKey = VoucherId;
            }
        }

        // TODO other fields here
    }
}
