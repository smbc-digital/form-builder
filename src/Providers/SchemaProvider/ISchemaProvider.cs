using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Providers.SchemaProvider
{
    public interface ISchemaProvider
    {
        Task<T> Get<T>(string schemaName);

        FormSchema Get(string schemaName);
    }
}