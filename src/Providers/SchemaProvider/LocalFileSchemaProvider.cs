using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Providers.SchemaProvider
{
    public class LocalFileSchemaProvider : ISchemaProvider
    {
        private readonly string _fileBaseFolder = $@".\DSL";
        private readonly LocalFileConfiguration _localFileConfig;

        public LocalFileSchemaProvider(IOptions<LocalFileConfiguration> localFileConfig)
        {
            _localFileConfig = localFileConfig.Value;
            if(!string.IsNullOrEmpty(_localFileConfig.SchemaBase))
                _fileBaseFolder = _localFileConfig.SchemaBase;
        }

        public T Get<T>(string schemaName)
        {
            var baseForm = System.IO.File.ReadAllText($@"{_fileBaseFolder}\{schemaName}.json");
            var obj = JsonConvert.DeserializeObject<T>(baseForm);
            return obj;
        }

        public FormSchema Get(string schemaName) => Get<FormSchema>(schemaName);

        async Task<T> ISchemaProvider.Get<T>(string schemaName)
        {
            var baseForm = System.IO.File.ReadAllText($@"{_fileBaseFolder}\{schemaName}.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(baseForm));
        }

        public Task<List<string>> IndexSchema() => Task.FromResult(System.IO.Directory.GetFiles(_fileBaseFolder).ToList());

        public Task<bool> ValidateSchemaName(string schemaName) => Task.FromResult(System.IO.Directory.GetFiles(_fileBaseFolder).ToList().Any(_ => _.Contains(schemaName, StringComparison.InvariantCultureIgnoreCase)));
    }
}