namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string Hours { get; set; } = string.Empty;
        public string Minutes { get; set; } = string.Empty;
        public string AmPm { get; set; } = string.Empty;
        public string CustomValidationMessageAmPm { get; set; } = string.Empty;
        public string ValidationMessageInvalidTime { get; set; } = string.Empty;
    }
}
