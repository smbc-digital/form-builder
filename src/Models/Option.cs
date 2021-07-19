namespace form_builder.Models
{
    public class Option
    {
        public string Text { get; set; }

        public string Value { get; set; }

        public string Hint { get; set; }

        public bool HasHint => !string.IsNullOrEmpty(Hint);

        public string ConditionalElementId { get; set; }

        public bool HasConditionalElement => !(ConditionalElementId is null);

        public bool Checked { get; set; }

        public bool Selected { get; set; }

        public string Divider { get; set; }

        public bool HasDivider => !string.IsNullOrEmpty(Divider);
    }
}