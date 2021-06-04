using System.Threading;
using System.Threading.Tasks;

namespace form_builder.Providers.FileStorage
{
    public interface IFileStorageProvider
    {
        string ProviderName { get; }

        string GetString(string key);

        void Remove(string key);

        Task SetStringAsync(string key, string value, int expiration, CancellationToken token = default);
    }
}
