﻿namespace form_builder.Providers.FileStorage
{
    public interface IFileStorageProvider
    {
        string ProviderName { get; }

        Task<string> GetString(string key);

        Task Remove(string key);

        Task SetStringAsync(string key, string value, int expiration, CancellationToken token = default);
    }
}
