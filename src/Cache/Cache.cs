using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Configuration;
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
        private readonly IDistributedCacheWrapper _distrbutedCache;
        private readonly ISchemaProvider _schemaProvider;
        private readonly IOptions<DistrbutedCacheConfiguration> _distrbutedCacheConfiguration;
        private readonly IConfiguration _configuration;

        public Cache(IDistributedCacheWrapper distrbutedCache, ISchemaProvider schemaProvider, IOptions<DistrbutedCacheConfiguration> distrbutedCacheConfiguration, IConfiguration configuration)
        {
            _distrbutedCache = distrbutedCache;
            _schemaProvider = schemaProvider;
            _distrbutedCacheConfiguration = distrbutedCacheConfiguration;
            _configuration = configuration;
        }

        public async Task<T> GetFromCacheOrDirectlyFromSchemaAsync<T>(string cacheKey, int minutes, ESchemaType type)
        {
            T result;
            var prefix = type == ESchemaType.PaymentConfiguration ? "payment-config/" : string.Empty;

            if (_distrbutedCacheConfiguration.Value.UseDistrbutedCache && minutes > 0)
            {
                var data = _distrbutedCache.GetString($"{type.ToESchemaTypePrefix(_configuration["ApplicationVersion"])}{cacheKey}");

                if(data == null)
                {
                    result = await _schemaProvider.Get<T>($"{prefix}{cacheKey}");

                    if (result != null)
                    {
                        await _distrbutedCache.SetStringAsync($"{type.ToESchemaTypePrefix(_configuration["ApplicationVersion"])}{cacheKey}", JsonConvert.SerializeObject(result), minutes);
                    };

                    return result;
                }

                return JsonConvert.DeserializeObject<T>(data);
            }
            
            return await _schemaProvider.Get<T>($"{prefix}{cacheKey}");
        }
    }
}
