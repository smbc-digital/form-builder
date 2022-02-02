namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string OptionalIfQuestionId { get; set; } = string.Empty;

        public string OptionalIfValue { get; set; } = string.Empty;

        public string OptionalIfNotValue { get; set; } = string.Empty;
    }
}