namespace form_builder.Configuration
{
    public class FileStorageProviderConfiguration
    {
        public const string ConfigValue = "FileStorageProvider";
        public string Type { get; set; }
        public string S3BucketName { get; set; }
    }
}