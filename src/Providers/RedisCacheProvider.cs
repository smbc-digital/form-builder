using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace form_builder.Providers
{
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly bool _useRedis;
        private readonly ConnectionMultiplexer _connection;

        public RedisCacheProvider(ConnectionMultiplexer connection, bool useRedis)
        {
            _useRedis = useRedis;
            _connection = connection;
        }

        public string GetString(string key)
        {
            if (_useRedis)
            {
                var db = GetDatabase();
                return db.StringGet(key);
            }

            return null;
        }

        public async Task<string> GetStringAsync(string key)
        {
            var db = _connection.GetDatabase();
            return await db.StringGetAsync(key);
        }

        public async Task<bool> Remove(string key)
        {
            if (_useRedis)
            {
                var db = GetDatabase();
                return await db.KeyDeleteAsync(key);
            }

            return true;
        }

        public void SetString(string key, string value, int minutes)
        {
            if (_useRedis)
            {
                var db = GetDatabase();
                db.StringSet(key, value, TimeSpan.FromMinutes(minutes));
            }
        }

        public string GetJson(string guid)
        {
            var viewModelDataFromRedis = GetString(guid);

            if (string.IsNullOrEmpty(viewModelDataFromRedis))
            {
                throw new Exception("could not retrieve data from redis");
            }

            return viewModelDataFromRedis;
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

        private IDatabase GetDatabase()
        {
            try
            {
                return _connection.GetDatabase();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create Redis database connection", ex);
            }
        }
    }
}
