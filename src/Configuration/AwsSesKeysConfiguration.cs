namespace form_builder.Configuration
{
    public class AwsSesKeysConfiguration
    {
        public const string ConfigValue = "Ses";
        public string Accesskey { get; set; }
        public string Secretkey { get; set; }

        public AwsSesKeysConfiguration(string accesskey, string secretkey)
        {
            Accesskey = accesskey;
            Secretkey = secretkey;
        }
    }
}