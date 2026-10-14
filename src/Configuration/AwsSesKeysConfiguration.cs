namespace form_builder.Configuration;

public class AwsSesKeysConfiguration(string accesskey, string secretkey)
{
    public const string ConfigValue = "Ses";
    public string Accesskey { get; set; } = accesskey;
    public string Secretkey { get; set; } = secretkey;
}