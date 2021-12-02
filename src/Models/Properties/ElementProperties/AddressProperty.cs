﻿using form_builder.Constants;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string AddressManualAddressLine1 { get; set; } = string.Empty;

        public string AddressManualAddressLine2 { get; set; } = string.Empty;

        public string AddressManualAddressTown { get; set; } = string.Empty;

        public string AddressManualAddressPostcode { get; set; } = string.Empty;

        public string AddressManualHint { get; set; } = string.Empty;

        public string AddressLabel { get; set; } = string.Empty;

        public string PostcodeLabel { get; set; } = "Postcode";

        public string AddressManualLabel { get; set; } = "Enter an address";

        public string AddressIAG { get; set; } = "You must live in Stockport.";

        public bool StockportPostcode { get; set; } = false;

        public bool DisplayNoResultsIAG { get; set; } = false;

        public bool Disabled { get; set; } = false;

        public string AddressManualLinkText { get; set; } = AddressConstants.ADDRESS_MANUAL_DISABLED_TEXT;

        public bool DisableManualAddress { get; set; } = false;

        public string NoManualAddressDetailText { get; set; }
    }
}