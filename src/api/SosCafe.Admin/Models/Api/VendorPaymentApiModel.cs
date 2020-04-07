using System;

namespace SosCafe.Admin.Models.Api
{
    public class VendorPaymentApiModel
    {
        public string PaymentId { get; set; }

        public DateTime PaymentDate { get; set; }

        public string BankAccountNumber { get; set; }

        public decimal GrossPayment { get; set; }

        public decimal Fees { get; set; }

        public decimal NetPayment { get; set; }
    }
}
