namespace form_builder.Configuration
{
    public class S3SchemaProviderConfiguration
    {
        public const string ConfigValue = "S3SchemaProvider";
        public string S3BucketFolderName { get; set; }
        public string S3BucketKey { get; set; }
    }
}