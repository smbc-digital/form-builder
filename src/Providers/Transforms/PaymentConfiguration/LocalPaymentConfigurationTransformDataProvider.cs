using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.PaymentConfiguration
{
    public class LocalPaymentConfigurationTransformDataProvider : IPaymentConfigurationTransformDataProvider
    {
        public async Task<T> Get<T>()
        {
            var data = System.IO.File.ReadAllText($@".\DSL\payment-config\paymentconfiguration.local.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(data));
        }
    }
}
