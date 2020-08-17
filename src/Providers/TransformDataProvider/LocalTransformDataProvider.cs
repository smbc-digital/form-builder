using System.Threading.Tasks;
using form_builder.Providers.TransformDataProvider;
using Newtonsoft.Json;

namespace form_builder.Providers.SchemaProvider
{
    public class LocalTransformDataProvider : ITransformDataProvider
    {
        public async Task<T> Get<T>(string name)
        {
            var data = System.IO.File.ReadAllText($@".\DSL\Lookups\{name}.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(data));
        }
    }
}