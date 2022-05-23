using System.Threading.Tasks;
using form_builder.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.PaymentConfiguration
{
    public class LocalPaymentConfigurationTransformDataProvider : IPaymentConfigurationTransformDataProvider
    {
        
        private readonly string _fileBaseFolder = $@".\DSL\payment-config";
        private readonly LocalFileConfiguration _localFileConfig;

        public LocalPaymentConfigurationTransformDataProvider(IOptions<LocalFileConfiguration> localFileConfig)
        {
            _localFileConfig = localFileConfig.Value;
            if(!string.IsNullOrEmpty(_localFileConfig.LocalPaymentConfigurationTransformBase))
                _fileBaseFolder = _localFileConfig.LocalPaymentConfigurationTransformBase;
        }
        
        public async Task<T> Get<T>()
        {
            return JsonConvert.DeserializeObject<T>(
                await System.IO.File.ReadAllTextAsync($@"{_fileBaseFolder}\paymentconfiguration.local.json"));
        }
    }
}
