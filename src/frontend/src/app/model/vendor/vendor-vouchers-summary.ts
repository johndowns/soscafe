export class VendorVouchersSummary {
  orderId: string;
  orderRef: string;
  orderDate: Date;
  customerName: string;
  customerEmailAddress: string;
  customerRegion: string;
  customerAcceptsMarketing: boolean;
  voucherDescription: string;
  voucherQuantity: number ;
  voucherIsDonation: boolean;
  voucherId: string;
  voucherGross: number ;
  voucherFees: number ;
  voucherNet: number;
  isRedeemed: boolean;
  dateRedeemed: Date;
}
