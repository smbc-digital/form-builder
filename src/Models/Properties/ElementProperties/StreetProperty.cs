namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string StreetProvider { get; set; }

        public string StreetIAG { get; set; } = "The street must be in Stockport. You can only enter letters A to Z and spaces, for example, Stockport Road.";

        public string StreetMissingText { get; set; }
    }
}