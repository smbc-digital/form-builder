namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string StreetProvider { get; set; }

        public string StreetIAG { get; set; } = "You must enter a Stockport street name and it must only include letters a to z and space. For example, Stockport Road.";
    }
}