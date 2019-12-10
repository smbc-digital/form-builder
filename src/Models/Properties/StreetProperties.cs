namespace form_builder.Models.Properties
{
    public class StreetProperties : BaseProperty
    {
        public string StreetProvider { get; set; }
        public string SelectLabel { get; set; } = string.Empty;
        public string StreetLabel { get; set; } = string.Empty;
        public string SelectCustomValidationMessage { get; set; } = string.Empty;
    }
}
