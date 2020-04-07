using Microsoft.WindowsAzure.Storage.Table;

namespace SosCafe.Admin
{
    public class VendorPaymentEntity : TableEntity
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

        private string paymentId;
        public string PaymentId
        {
            get
            {
                return paymentId;
            }
            set
            {
                paymentId = value;
                RowKey = PaymentId;
            }
        }

        // TODO other fields here
    }
}
