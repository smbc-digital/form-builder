﻿namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public string AddressManualAddressLine1 { get; set; } = string.Empty;
        public string AddressManualAddressLine2 { get; set; } = string.Empty;
        public string AddressManualAddressTown { get; set; } = string.Empty;
        public string AddressManualAddressPostcode { get; set; } = string.Empty;
        public string AddressManualHint { get; set; } = string.Empty;
        public string AddressLabel { get; set; } = "Select the address below";
        public string PostcodeLabel { get; set; } = "Enter your postcode";
        public string AddressProvider { get; set; }
        public bool? Postcode { get; set; }
        public bool? StockportPostcode { get; set; }
        public bool DisplayNoResultsIAG { get; set; } = false;
    }
}
