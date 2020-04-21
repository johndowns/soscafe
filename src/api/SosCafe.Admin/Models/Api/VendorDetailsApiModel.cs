﻿using System;

namespace SosCafe.Admin.Models.Api
{
    public class VendorDetailsApiModel
    {
        public string Id { get; set; }

        public string BusinessName { get; set; }

        public DateTime RegisteredDate { get; set; }

        public string ContactName { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string BankAccountNumber { get; set; }

        public bool HasAgreedToTerms { get; set; }

        public bool IsClickAndCollect { get; set; }
        public bool IsPickup { get; set; }
        public bool IsDelivery { get; set; }
    }
}
