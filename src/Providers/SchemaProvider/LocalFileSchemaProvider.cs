using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using Newtonsoft.Json;

namespace form_builder.Providers.SchemaProvider
{
    public class LocalFileSchemaProvider : ISchemaProvider
    {
        public T Get<T>(string schemaName)
        {
            var baseForm = System.IO.File.ReadAllText($@".\DSL\{schemaName}.json");
            var obj = JsonConvert.DeserializeObject<T>(baseForm);
            return obj;
        }

        public FormSchema Get(string schemaName) => Get<FormSchema>(schemaName);

        async Task<T> ISchemaProvider.Get<T>(string schemaName)
        {
            var baseForm = System.IO.File.ReadAllText($@".\DSL\{schemaName}.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(baseForm));
        }

        public Task<List<string>> IndexSchema() => Task.FromResult(System.IO.Directory.GetFiles($@".\DSL").ToList());

        public Task<bool> ValidateSchemaName(string schemaName) => Task.FromResult(System.IO.Directory.GetFiles($@".\DSL").ToList().Any(_ => _.Contains(schemaName, StringComparison.InvariantCultureIgnoreCase)));
    }
}