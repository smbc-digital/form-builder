namespace form_builder.Configuration
{
    public class FileStorageProviderConfiguration
    {
        public const string ConfigValue = "NotifyConfiguration";
        public string Type { get; set; }
        public string S3BucketName { get; set; }
    }
}