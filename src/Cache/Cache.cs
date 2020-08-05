using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Cache
{
    public interface ICache
    {
        Task<T> GetFromCacheOrDirectlyFromSchemaAsync<T>(string cacheKey, int minutes, ESchemaType type);
    }
    public class Cache : ICache
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly ISchemaProvider _schemaProvider;
        private readonly IOptions<DistributedCacheConfiguration> _distributedCacheConfiguration;

        public Cache(IDistributedCacheWrapper distributedCache, ISchemaProvider schemaProvider, IOptions<DistributedCacheConfiguration> distributedCacheConfiguration)
        {
            _distributedCache = distributedCache;
            _schemaProvider = schemaProvider;
            _distributedCacheConfiguration = distributedCacheConfiguration;
        }

        public async Task<T> GetFromCacheOrDirectlyFromSchemaAsync<T>(string cacheKey, int minutes, ESchemaType type)
        {
            T result;
            var prefix = type == ESchemaType.PaymentConfiguration ? "payment-config/" : string.Empty;

            if (_distributedCacheConfiguration.Value.UseDistributedCache && minutes > 0)
            {
                var data = _distributedCache.GetString($"{type.ToESchemaTypePrefix()}{cacheKey}");

                if(data == null)
                {
                    result = await _schemaProvider.Get<T>($"{prefix}{cacheKey}");

                    if (result != null)
                        await _distributedCache.SetStringAsync($"{type.ToESchemaTypePrefix()}{cacheKey}", JsonConvert.SerializeObject(result), minutes);

                    return result;
                }

                return JsonConvert.DeserializeObject<T>(data);
            }
            
            return await _schemaProvider.Get<T>($"{prefix}{cacheKey}");
        }
    }
}