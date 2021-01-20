namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string StreetProvider { get; set; }

        public string StreetIAG { get; set; } = "It must be a Stockport street name. For example, Stockport Road.";
    }
}