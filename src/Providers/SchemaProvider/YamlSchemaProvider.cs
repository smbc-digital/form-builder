using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace form_builder.Providers.SchemaProvider
{
    public class YamlSchemaProvider : ISchemaProvider
    {
        public T Get<T>(string schemaName)
        {
            var baseForm = File.ReadAllText($@".\DSL\Yaml\{schemaName}.yml");

            var jsonObject = YmlToJson(baseForm);

            return JsonConvert.DeserializeObject<T>(jsonObject);
        }

        public FormSchema Get(string schemaName) => Get<FormSchema>(schemaName);

        async Task<T> ISchemaProvider.Get<T>(string schemaName)
        {
            var baseForm = File.ReadAllText($@".\DSL\Yaml\{schemaName}.yml");

            var jsonObject = YmlToJson(baseForm);

            return await Task.FromResult(JsonConvert.DeserializeObject<T>(jsonObject));
        }

        private string YmlToJson(string baseForm)
        {
            var yamlObject = new Deserializer().Deserialize(new StringReader(baseForm));
            
            JsonSerializer js = new();
            StringWriter jsonObject = new();
            js.Serialize(jsonObject, yamlObject);
            return jsonObject.ToString();
        }

        public Task<List<string>> IndexSchema() => Task.FromResult(System.IO.Directory.GetFiles($@".\DSL\Yaml").ToList());

        public Task<bool> ValidateSchemaName(string schemaName) => Task.FromResult(System.IO.Directory.GetFiles($@".\DSL\Yaml").ToList().Any(_ => _.Contains(schemaName, StringComparison.InvariantCultureIgnoreCase)));
    }
}
