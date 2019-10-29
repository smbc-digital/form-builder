using form_builder.Models;
using System.Threading.Tasks;

namespace form_builder.Providers
{
    public interface ISchemaProvider
    {
        Task<T> Get<T>(string schemaName);
        FormSchema Get(string schemaName);
    }
}