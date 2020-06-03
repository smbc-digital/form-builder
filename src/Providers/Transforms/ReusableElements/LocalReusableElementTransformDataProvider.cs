using System.Threading.Tasks;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.ReusableElements
{
    public class LocalReusableElementTransformDataProvider : IReusableElementTransformDataProvider
    {
        public async Task<T> Get<T>(string name)
        {
            var data = System.IO.File.ReadAllText($@".\DSL\Elements\{name}.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(data));
        }
    }
}