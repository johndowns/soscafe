namespace SosCafe.Admin.Entities
{
    public class VendorUserAssignmentEntity : SosCafeEntity
    {
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

        public string VendorName { get; set; }

        private string userId;
        public string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
                PartitionKey = userId.CleanStringForPartitionKey().ToUpper();
            }
        }
    }
}
