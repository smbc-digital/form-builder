using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Models.Elements;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.ReusableElements
{
    public class LocalReusableElementTransformDataProvider : IReusableElementTransformDataProvider
    {
        private readonly string _fileBaseFolder = $@".\DSL\Elements";
        private readonly LocalFileConfiguration _localFileConfig;

        public LocalReusableElementTransformDataProvider(IOptions<LocalFileConfiguration> localFileConfig)
        {
            _localFileConfig = localFileConfig.Value;
            if(!string.IsNullOrEmpty(_localFileConfig.ReusableElementTransformBase))
                _fileBaseFolder = _localFileConfig.ReusableElementTransformBase;
        }

        public async Task<IElement> Get(string name)
        {
            var data = System.IO.File.ReadAllText($@"{_fileBaseFolder}\{name}.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<Element>(data));
        }
    }
}