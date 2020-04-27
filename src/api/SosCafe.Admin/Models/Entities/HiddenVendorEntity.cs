namespace SosCafe.Admin.Entities
{
    public class HiddenVendorEntity : SosCafeEntity
    {
        public HiddenVendorEntity()
        {
            PartitionKey = "Vendors";
        }

        private string vendorShopifyId;
        public string VendorShopifyId
        {
            get
            {
                return vendorShopifyId;
            }
            set
            {
                vendorShopifyId = value;
                RowKey = vendorShopifyId;
            }
        }
    }
}
