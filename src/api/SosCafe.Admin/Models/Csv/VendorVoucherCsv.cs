using System;
using CsvHelper.Configuration.Attributes;

namespace SosCafe.Admin.Csv
{
    public class VendorVoucherCsv
    {
        [Name("Vendor ID")]
        public string VendorId { get; set; }

        [Name("Order ID")]
        public string OrderId { get; set; }

        [Name("Order Ref")]
        public string OrderRef { get; set; }

        [Name("Order Date")]
        public DateTime OrderDate { get; set; }

        [Name("Customer Name")]
        public string CustomerName { get; set; }

        [Name("Customer Email")]
        public string CustomerEmailAddress { get; set; }

        [Name("Customer Region")]
        public string CustomerRegion { get; set; }

        [Name("Customer Accepts Marketing")]
        public string CustomerAcceptsMarketing { get; set; }

        [Name("Voucher Description")]
        public string VoucherDescription { get; set; }

        [Name("Voucher Quantity")]
        public int VoucherQuantity { get; set; }

        [Name("Voucher is Donation")]
        public string VoucherIsDonation { get; set; }

        [Name("Voucher ID")]
        public string VoucherId { get; set; }

        [Name("Voucher Gross")]
        public string VoucherGross { get; set; }

        [Name("Voucher Fees")]
        public string VoucherFees { get; set; }

        [Name("Voucher Net")]
        public string VoucherNet { get; set; }
    }
}
