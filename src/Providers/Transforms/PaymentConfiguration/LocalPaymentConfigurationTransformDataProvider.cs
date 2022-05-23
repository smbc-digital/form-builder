using System.Threading.Tasks;
using form_builder.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.PaymentConfiguration
{
    public class LocalPaymentConfigurationTransformDataProvider : IPaymentConfigurationTransformDataProvider
    {
        
        private readonly string _fileBaseFolder = $@".\DSL\payment-config";
        private readonly LocalFileConfiguration _localFileConfig;

        private readonly IWebHostEnvironment _environment;

        public LocalPaymentConfigurationTransformDataProvider(IOptions<LocalFileConfiguration> localFileConfig, IWebHostEnvironment environment)
        {
            _localFileConfig = localFileConfig.Value;
            _environment = environment;
            if(!string.IsNullOrEmpty(_localFileConfig.LocalPaymentConfigurationTransformBase))
                _fileBaseFolder = _localFileConfig.LocalPaymentConfigurationTransformBase;
        }
        
        public async Task<T> Get<T>()
        {
            return JsonConvert.DeserializeObject<T>(
                await System.IO.File.ReadAllTextAsync($@"{_fileBaseFolder}\paymentconfiguration.{_environment.EnvironmentName}.json"));
        }
    }
}
