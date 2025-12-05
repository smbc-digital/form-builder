using form_builder.Enum;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public ESize Width { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string PrefixAriaLabel { get; set; }
        public string SuffixAriaLabel { get; set; }
        public bool Decimal { get; set; } = false;
        public int DecimalPlaces { get; set; } = 2;
    }
}