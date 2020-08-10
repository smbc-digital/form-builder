using System.Threading.Tasks;

namespace form_builder.Providers.Transforms.Lookups
{
    public interface ILookupTransformDataProvider
    {
        Task<T> Get<T>(string schemaName);
    }
}