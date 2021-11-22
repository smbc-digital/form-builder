using System.Threading.Tasks;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.Lookups
{
    public class LocalLookupTransformDataProvider : ILookupTransformDataProvider
    {
        public async Task<T> Get<T>(string name)
        {
            var data = System.IO.File.ReadAllText($@".\DSL\Lookups\{name}.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(data));
        }
    }
}