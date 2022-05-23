using System.Threading.Tasks;
using form_builder.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.Lookups
{
    public class LocalLookupTransformDataProvider : ILookupTransformDataProvider
    {
        private readonly string _fileBaseFolder = $@".\DSL\Lookups";
        private readonly LocalFileConfiguration _localFileConfig;

        public LocalLookupTransformDataProvider(IOptions<LocalFileConfiguration> localFileConfig)
        {
            _localFileConfig = localFileConfig.Value;
            if(!string.IsNullOrEmpty(_localFileConfig.LookupBase))
                _fileBaseFolder = _localFileConfig.LookupBase;
        }
        
        public async Task<T> Get<T>(string name)
        {
            var data = System.IO.File.ReadAllText($@"{_fileBaseFolder}\{name}.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(data));
        }
    }
}