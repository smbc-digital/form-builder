using form_builder.Constants;

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

        public string AddressProvider { get; set; }

        public string AddressIAG { get; set; } = "You must live in Stockport.";

        public bool StockportPostcode { get; set; } = false;

        public bool FullUKPostcode { get; set; } = false;

        public bool ValidatePostcode { get; set; } = true;

        public bool DisplayNoResultsIAG { get; set; } = false;

        public bool Disabled { get; set; } = false;

        public string AddressManualLinkText { get; set; } = AddressConstants.ADDRESS_MANUAL_DISABLED_TEXT;

        public bool DisableManualAddress { get; set; } = false;

        public string NoManualAddressDetailText { get; set; }

        public int AddressManualLineMaxLength { get; set; } = 95;

        public int AddressManualPostcodeMaxLength { get; set; } = 8;
    }
}