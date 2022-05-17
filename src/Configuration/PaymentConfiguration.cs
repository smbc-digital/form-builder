namespace form_builder.Configuration
{
    public class PaymentConfiguration
    {
        public const string ConfigValue = "PaymentConfiguration";

        public readonly string FakeProviderName = "Fake";
        
        public bool FakePayment { get; set; } = false;
    }
}