namespace form_builder.Configuration
{
    public class AwsSesKeysConfiguration
    {
        public string Accesskey { get; set; }
        public string Secretkey { get; set; }

        public AwsSesKeysConfiguration(string accesskey, string secretkey)
        {
            Accesskey = accesskey;
            Secretkey = secretkey;
        }

        public bool IsValid() => !string.IsNullOrWhiteSpace(Accesskey) && !string.IsNullOrWhiteSpace(Secretkey);
    }
}
