namespace form_builder.Models.Properties
{
    public class AddressProperties : BaseProperty
    {
        public string AddressProvider { get; set; }
        public string AddressLabel { get; set; } = string.Empty;
        public string AddressManualAddressLine1 { get; set; } = string.Empty;
        public string AddressManualAddressLine2 { get; set; } = string.Empty;
        public string AddressManualAddressTown { get; set; } = string.Empty;
        public string AddressManualAddressPostcode { get; set; } = string.Empty;
        public string PostcodeLabel { get; set; } = string.Empty;
        public string SelectHint { get; set; } = string.Empty;
        public string SelectCustomValidationMessage { get; set; } = string.Empty;
    }
}
