namespace form_builder.Models.Properties
{
    public class BaseProperty
    {
        public string QuestionId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; }
        public string Label { get; set; }
        public bool Optional { get; set; } = false;
        public string CustomValidationMessage { get; set; } = string.Empty;
        public string MaxLength { get; set; }
        public bool? Email { get; set; }
        public bool? Postcode { get; set; }
        public string Regex { get; set; } = string.Empty;
        public bool? StockportPostcode { get; set; }
        public bool Numeric { get; set; } = false;
        public string Hint { get; set; } = string.Empty;
        public string RequiredIf { get; set; } = string.Empty;
    }
}
