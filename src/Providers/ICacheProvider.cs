using System;
using System.Threading.Tasks;

namespace form_builder.Providers
{
    public interface ICacheProvider
    {
        Task<bool> Remove(string key);

        void SetString(string key, string value, int minutes);

        string GetString(string key);

        Task<string> GetStringAsync(string key);

        string GetJson(string guid);

        T GetModel<T>(string guid);

        void UpdateFromModel<T>(T model, Guid redisGuid);

        void RemoveKey(string guid);
    }
}