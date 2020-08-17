namespace form_builder.Models
{
    public class Option
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Hint { get; set; }
        public bool Checked { get; set; }
        public bool Selected { get; set; }
    }
}