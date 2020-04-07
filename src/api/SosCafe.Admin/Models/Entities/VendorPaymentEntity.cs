using System;

namespace SosCafe.Admin.Entities
{
    public class VendorPaymentEntity : SosCafeEntity
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
                var cleanedPaymentId = PaymentId.CleanStringForPartitionKey();
                RowKey = cleanedPaymentId;
            }
        }

        public DateTime PaymentDate { get; set; }

        public string BankAccountNumber { get; set; }

        public decimal GrossPayment { get; set; }

        public decimal Fees { get; set; }

        public decimal NetPayment { get; set; }
    }
}
