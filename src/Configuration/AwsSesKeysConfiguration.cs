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
    }
}
