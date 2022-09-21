using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.PaymentConfiguration
{
    public class LocalPaymentConfigurationTransformDataProvider : IPaymentConfigurationTransformDataProvider
    {
        public async Task<T> Get<T>()
        {
            return JsonConvert.DeserializeObject<T>(
                await System.IO.File.ReadAllTextAsync($@".\DSL\payment-config\paymentconfiguration.local.json"));
        }
    }
}
