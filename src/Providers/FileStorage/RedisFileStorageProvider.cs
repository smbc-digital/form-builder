﻿using System;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Caching.Distributed;

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

        public RedisFileStorageProvider(IDistributedCacheWrapper distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public string GetString(string key) => _distributedCache.GetString(key);

        public void Remove(string key) => _distributedCache.Remove(key);

        public Task SetStringAsync(string key, string value, int expiration, CancellationToken token = default)
        {
            return _distributedCache.SetStringAsync(key, value, expiration, token);
        }
    }
}