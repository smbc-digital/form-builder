using System.Threading.Tasks;
using form_builder.Enum;

namespace form_builder.Cache
{
    public interface ICache
    {
        Task<T> GetFromCacheOrDirectlyFromSchemaAsync<T>(string cacheKey, int minutes, ESchemaType type);
    }
}
