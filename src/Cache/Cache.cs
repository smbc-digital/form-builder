using System.Threading.Tasks;
using form_builder.Extensions;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;

namespace form_builder.Cache
{
    public enum ESchemaType {
        Unknown,
        FormJson,
        PaymentConfiguration
    }

    public interface ICache
    {
        Task<T> GetFromCacheOrDirectlyFromSchemaAsync<T>(string cacheKey, int minutes, bool useDistrbutedCache, ESchemaType type);
    }
    public class Cache : ICache
    {
        private readonly IDistributedCacheWrapper _distrbutedCache;
        private readonly ISchemaProvider _schemaProvider;

        public Cache(IDistributedCacheWrapper distrbutedCache, ISchemaProvider schemaProvider)
        {
            _distrbutedCache = distrbutedCache;
            _schemaProvider = schemaProvider;
        }

        public async Task<T> GetFromCacheOrDirectlyFromSchemaAsync<T>(string cacheKey, int minutes, bool useDistrbutedCache, ESchemaType type)
        {
            T result;
            var prefix = type == ESchemaType.PaymentConfiguration ? "payment-config/" : string.Empty;

            if (useDistrbutedCache && minutes > 0)
            {
                var data = _distrbutedCache.GetString($"{type.ToESchemaTypePrefix()}{cacheKey}");

                if(data == null)
                {
                    result = await _schemaProvider.Get<T>($"{prefix}{cacheKey}");

                    if (result != null)
                    {
                        await _distrbutedCache.SetStringAsync($"{type.ToESchemaTypePrefix()}{cacheKey}", JsonConvert.SerializeObject(result), minutes);
                    };

                    return result;
                }

                return JsonConvert.DeserializeObject<T>(data);
            }
            else
            {
                return await _schemaProvider.Get<T>($"{prefix}{cacheKey}");
            }
        }
    }
}
