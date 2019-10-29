using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace form_builder.Providers
{
    public class LocalSessionCacheProvider : ICacheProvider
    {
        private readonly HttpContext _httpContext;

        public LocalSessionCacheProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public string GetString(string key)
        {
            return _httpContext.Session.GetString(key);
        }

        public async Task<string> GetStringAsync(string key)
        {
           return await Task.Run<string>(() => { return GetString(key); });
        }

        public async Task<bool> Remove(string key)
        {
            await Task.Run(() => { _httpContext.Session.Remove(key); });
            return true;
        }

        public void SetString(string key, string value, int minutes)
        {
            _httpContext.Session.SetString(key, value);
        }

        public string GetJson(string guid)
        {
            var viewModelData = GetString(guid);

            if (string.IsNullOrEmpty(viewModelData))
            {
                throw new Exception("could not retrieve data from redis");
            }

            return viewModelData;
        }

        public T GetModel<T>(string guid)
        {
            var json = GetJson(guid);
            var model = JsonConvert.DeserializeObject<T>(json);
            return model;
        }

        public void UpdateFromModel<T>(T model, Guid redisGuid)
        {
            var dataString = JsonConvert.SerializeObject(model);
            SetString(redisGuid.ToString(), dataString, 30);
        }

        public void RemoveKey(string guid)
        {
            Remove(guid).Wait();
        }
    }
}