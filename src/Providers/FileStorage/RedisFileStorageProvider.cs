using form_builder.Providers.StorageProvider;

namespace form_builder.Providers.FileStorage
{
    /**
     * IDistributedCacheWrapper
     * Due to IDistributedCache using static methods and the inability to moq static methods. 
     * This is a wrapper around IDistributedCache to allow testing of static methods
     **/
    public class RedisFileStorageProvider : IFileStorageProvider
    {
        private readonly IDistributedCacheWrapper _distributedCache;

        public string ProviderName { get => "Redis"; }

        public RedisFileStorageProvider(IDistributedCacheWrapper distributedCache) =>
            _distributedCache = distributedCache;


        public async Task<string> GetString(string key) => await Task.Run(() =>
            { return _distributedCache.GetString(key); });

        public async Task Remove(string key) => await _distributedCache.RemoveAsync(key);

        public Task SetStringAsync(string key, string value, int expiration, CancellationToken token = default) =>
             _distributedCache.SetStringAsync(key, value, expiration, token);
    }
}
