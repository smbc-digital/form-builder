﻿using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace form_builder.Providers.StorageProvider
{
    public interface IDistributedCacheWrapper
    {
        string GetString(string key);

        Task<byte[]> GetAsync(string key, CancellationToken token = default);

        void Refresh(string key);

        Task RefreshAsync(string key, CancellationToken token = default);

        void Remove(string key);

        Task RemoveAsync(string key, CancellationToken token = default);

        void Set(string key, byte[] value, DistributedCacheEntryOptions options);

        Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default);

        Task SetStringAsync(string key, string value, CancellationToken token = default);

        byte[] Get(string key);
    }
}
