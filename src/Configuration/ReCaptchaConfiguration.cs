namespace form_builder.Configuration
{
    public class ReCaptchaConfiguration
    {
        public const string ConfigValue = "ReCaptchaConfiguration";
        public string ApiVerificationEndpoint { get; set; }
        public string AuthToken { get; set; }
        public string SiteKey { get; set; }
    }
}
