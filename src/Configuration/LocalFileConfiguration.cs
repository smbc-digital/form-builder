namespace form_builder.Configuration
{
    public class LocalFileConfiguration
    {
        public const string ConfigValue = "LocalFileConfiguration";
        public string SchemaBase { get; set; }
        public string LookupBase { get; set; }
        public string ReusableElementTransformBase { get; set; }
        public string LocalPaymentConfigurationTransformBase  { get; set; }

    }
}