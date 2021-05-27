using System;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace form_builder.Providers.FileStorage
{
    /**
     * IDistributedCacheWrapper
     * Due to IDistributedCache using static methods and the inability to moq static methods. 
     * This is a wrapper around IDistributedCache to allow testing of static methods
     **/
    public class FileStorageProvider : IFileStorageProvider
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public string ProviderName { get => "DistrbutedCache"; }

        public FileStorageProvider(IDistributedCache distributedCache, IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration)
        {
            _distributedCache = distributedCache;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        }

        public string GetString(string key) => _distributedCache.GetString(key);

        public void Remove(string key) => _distributedCache.Remove(key);

        public Task SetStringAsync(string key, string value, int expiration, CancellationToken token = default)
        {
            var distributedCacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(expiration)
            };

            return _distributedCache.SetStringAsync(key, value, distributedCacheOptions, token);
        }
    }
}
