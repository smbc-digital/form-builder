namespace form_builder.Models.Properties
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
                public string AddressManualLabel { get; set; } = "Select an address";
                public string AddressProvider { get; set; }
                public bool? Postcode { get; set; }
                public string AddressIAG { get; set; } = "You must live in Stockport.";
                public bool? StockportPostcode { get; set; }
                public bool DisplayNoResultsIAG { get; set; } = false;
                public bool Disabled { get; set; } = false;
                public string AddressManualLinkText { get; set; } = "I can't find my address in the list";
        }       
}
