using form_builder.Models;

namespace form_builder.Providers.SchemaProvider
{
    public interface ISchemaProvider
    {
        Task<T> Get<T>(string schemaName);

        FormSchema Get(string schemaName);

        Task<bool> ValidateSchemaName(string schemaName);

        Task<List<string>> IndexSchema();
    }
}