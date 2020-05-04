using System;
using CsvHelper.Configuration.Attributes;

namespace SosCafe.Admin.Csv
{
    public class AdminVendorVoucherCsv
    {
        [Name("Vendor ID")]
        [Index(0)]
        public string VendorId { get; set; }

        [Name("Order Number ID")]
        [Index(1)]
        public string OrderId { get; set; }

        [Name("Order reference number")]
        [Index(2)]
        public string OrderRef { get; set; }

        [Name("Customer name")]
        [Index(3)]
        public string CustomerName { get; set; }

        [Name("Customer email address")]
        [Index(4)]
        public string CustomerEmailAddress { get; set; }

        [Name("Order date & time")]
        [Index(5)]
        public DateTime OrderDate { get; set; }

        [Name("Line item ID	")]
        [Index(6)]
        public string LineItemId { get; set; }

        [Name("Voucher ID")]
        [Index(7)]
        public string VoucherId { get; set; }

        [Name("Payment gateway")]
        [Index(8)]
        public string PaymentGateway { get; set; }

        [Name("Quantity")]
        [Index(9)]
        public int VoucherQuantity { get; set; }

        [Name("isDonation")]
        [Index(10)]
        public string VoucherIsDonation { get; set; }

        [Name("Gross sales")]
        [Index(11)]
        public string VoucherGross { get; set; }

        [Name("CC Fees")]
        [Index(12)]
        public string VoucherFees { get; set; }

        [Name("Net Sales")]
        [Index(13)]
        public string VoucherNet { get; set; }

        [Name("isRefunded")]
        [Index(14)]
        public string IsRefunded { get; set; }
    }
}
