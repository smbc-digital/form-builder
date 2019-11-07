using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace form_builder.Providers.StorageProvider
{
    /**
     * IDistributedCacheWrapper
     * Due to IDistributedCache using static methods and the inability to moq static methods. 
     * This is a wrapper around IDistributedCache to allow testing of static methods
     **/
    public class DistributedCacheWrapper : IDistributedCacheWrapper
    {
        private readonly IDistributedCache _distributedCache;

        public DistributedCacheWrapper(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public string GetString(string key)
        {
            return _distributedCache.GetString(key);
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return _distributedCache.GetAsync(key, token);
        }

        public void Refresh(string key)
        {
            _distributedCache.Refresh(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return _distributedCache.RefreshAsync(key, token);
        }

        public void Remove(string key)
        {
            _distributedCache.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return _distributedCache.RefreshAsync(key, token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _distributedCache.Set(key, value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return _distributedCache.SetAsync(key, value, options, token);
        }

        public Task SetStringAsync(string key, string value, CancellationToken token = default)
        {
            return _distributedCache.SetStringAsync(key, value, token);
        }

        public byte[] Get(string key)
        {
            return _distributedCache.Get(key);
        }
    }
}
