using form_builder.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace form_builder.Providers.StorageProvider;

/**
 * IDistributedCacheWrapper
 * Due to IDistributedCache using static methods and the inability to moq static methods.
 * This is a wrapper around IDistributedCache to allow testing of static methods
 **/
public class DistributedCacheWrapper : IDistributedCacheWrapper
{
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

    public DistributedCacheWrapper(IDistributedCache distributedCache, IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration)
    {
        _distributedCache = distributedCache;
        _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
    }

    public string GetString(string key) => _distributedCache.GetString(key);

    public Task<byte[]> GetAsync(string key, CancellationToken token = default) => _distributedCache.GetAsync(key, token);

    public void Refresh(string key) => _distributedCache.Refresh(key);

    public Task RefreshAsync(string key, CancellationToken token = default) => _distributedCache.RefreshAsync(key, token);

    public void Remove(string key) => _distributedCache.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default) => _distributedCache.RemoveAsync(key, token);

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _distributedCache.Set(key, value, options);

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
        _distributedCache.SetAsync(key, value, options, token);

    public Task SetStringAsync(string key, string value, CancellationToken token = default)
    {
        var distributedCacheOptions = new DistributedCacheEntryOptions();        

        distributedCacheOptions.SlidingExpiration = TimeSpan.FromMinutes(_distributedCacheExpirationConfiguration.UserData);

        return _distributedCache.SetStringAsync(key, value, distributedCacheOptions, token);
    }

    public Task SetStringAsync(string key, string value, int expiration, CancellationToken token = default)
    {
        var distributedCacheOptions = new DistributedCacheEntryOptions();

        distributedCacheOptions.SlidingExpiration = TimeSpan.FromMinutes(expiration);

        return _distributedCache.SetStringAsync(key, value, distributedCacheOptions, token);
    }

    public Task SetStringAbsoluteAsync(string key, string value, int expiration, CancellationToken token = default)
    {
        var distributedCacheOptions = new DistributedCacheEntryOptions();

        distributedCacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiration);

        return _distributedCache.SetStringAsync(key, value, distributedCacheOptions, token);
    }

    public byte[] Get(string key) => _distributedCache.Get(key);
}