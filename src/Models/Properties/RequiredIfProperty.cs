namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public string RequiredIf { get; set; } = string.Empty;
        public string RequiredIfValidationMessage { get; set; } = string.Empty;
    }
}
