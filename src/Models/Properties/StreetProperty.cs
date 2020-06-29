namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public string StreetProvider { get; set; }

        public string StreetIAG { get; set; } = "It must be a Stockport street name.";
    }
}
