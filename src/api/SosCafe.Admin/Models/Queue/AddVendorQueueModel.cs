using System;

namespace SosCafe.Admin.Models.Queue
{
    public class AddVendorQueueModel
    {
        public string BusinessName { get; set; }
        public string Type { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string BusinessPhotoUrl { get; set; }
        public string BankAccountNumber { get; set; }
        public DateTime DateAcceptedTerms { get; set; }
        public string EmailAddress { get; set; }
        public string ContactName { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}
