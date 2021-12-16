using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace form_builder.Providers.SchemaProvider
{
    public class YamlSchemaProvider : ISchemaProvider
    {
        public T Get<T>(string schemaName)
        {
            var baseForm = System.IO.File.ReadAllText($@".\DSL\Yaml\{schemaName}.yml");

            var deserializer = new DeserializerBuilder().Build();

            return deserializer.Deserialize<T>(baseForm);
        }

        public FormSchema Get(string schemaName) => Get<FormSchema>(schemaName);

        async Task<T> ISchemaProvider.Get<T>(string schemaName)
        {
            var baseForm = System.IO.File.ReadAllText($@".\DSL\Yaml\{schemaName}.yml");

            var deserializer = new DeserializerBuilder().Build();

            return await Task.FromResult(deserializer.Deserialize<T>(baseForm));
        }

        public Task<List<string>> IndexSchema() => Task.FromResult(System.IO.Directory.GetFiles($@".\DSL\Yaml").ToList());

        public Task<bool> ValidateSchemaName(string schemaName) => Task.FromResult(System.IO.Directory.GetFiles($@".\DSL\Yaml").ToList().Any(_ => _.Contains(schemaName, StringComparison.InvariantCultureIgnoreCase)));
    }
}
