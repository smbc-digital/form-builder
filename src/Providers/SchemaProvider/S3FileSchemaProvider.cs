using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using form_builder.Extensions;
using form_builder.Gateways;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using form_builder.Constants;
using Microsoft.Extensions.Logging;
using Amazon.S3.Model;
using System.Collections.Generic;
using form_builder.Configuration;
using Microsoft.Extensions.Options;

namespace form_builder.Providers.SchemaProvider
{
    public class S3FileSchemaProvider : ISchemaProvider
    {
        private readonly IS3Gateway _s3Gateway;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ISchemaProvider> _logger;
        private readonly IDistributedCacheWrapper _distributedCacheWrapper;
        private readonly DistributedCacheConfiguration _distributedCacheConfiguration;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public S3FileSchemaProvider(IS3Gateway s3Service, 
            IWebHostEnvironment environment, 
            IDistributedCacheWrapper distributedCacheWrapper,
            IConfiguration configuration,
            IOptions<DistributedCacheConfiguration> distributedCacheConfiguration,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            ILogger<ISchemaProvider> logger)
        {
            _s3Gateway = s3Service;
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
            _distributedCacheWrapper = distributedCacheWrapper;
            _distributedCacheConfiguration = distributedCacheConfiguration.Value;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        }

        public async Task<T> Get<T>(string schemaName)
        {
            try
            {
                var s3Result = await _s3Gateway.GetObject(_configuration["S3BucketKey"], $"{_environment.EnvironmentName.ToS3EnvPrefix()}/{_configuration["ApplicationVersion"]}/{schemaName}.json");

                using (Stream responseStream = s3Result.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    var responseBody = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }
            }
            catch (AmazonS3Exception e)
            {
                var ex = new Exception($"S3FileSchemaProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {_environment.EnvironmentName.ToS3EnvPrefix()}/{_configuration["ApplicationVersion"]}/{schemaName}", e);
                throw ex;
            }
            catch (Exception e)
            {
                var ex = new Exception($"S3FileSchemaProvider: An error has occured while attempting to deserialize object, Exception: {e.Message}", e);
                throw ex;
            }
        }

        public FormSchema Get(string schemaName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> IndexSchema()
        {
            if(!_distributedCacheConfiguration.UseDistributedCache)
            {
                _logger.LogWarning($"S3FileSchemaProvider::IndexSchema, A request to index form schema was made but UseDistributedCache is disabled");
                return new List<string>();
            }

            var result = new ListObjectsV2Response();
            try
            {
                result = await _s3Gateway.ListObjectsV2(_configuration["S3BucketKey"], $"{_environment.EnvironmentName.ToS3EnvPrefix()}/{_configuration["ApplicationVersion"]}");
            }
            catch (Exception e)
            {
                _logger.LogWarning($"S3FileSchemaProvider::IndexSchema, Failed to retrieve list of forms from s3 bucket {_environment.EnvironmentName.ToS3EnvPrefix()}/{_configuration["ApplicationVersion"]}, Exception: {e.Message}");
                return new List<string>();
            }

            var indexKeys = result.S3Objects.Select(_ => _.Key).ToList();
            _ = _distributedCacheWrapper.SetStringAsync(CacheConstants.INDEX_SCHEMA, JsonConvert.SerializeObject(indexKeys), _distributedCacheExpirationConfiguration.Index);
            
            return indexKeys;
        }

        public bool ValidateSchemaName(string schemaName)
        {
            if(!_distributedCacheConfiguration.UseDistributedCache)
                return true;

            var cachedIndexSchema = _distributedCacheWrapper.GetString(CacheConstants.INDEX_SCHEMA);
            
            if(string.IsNullOrEmpty(cachedIndexSchema))
                return true;

            var indexSchema = JsonConvert.DeserializeObject<List<string>>(cachedIndexSchema);
            return indexSchema.Any(_ => _.Contains(schemaName));
        }
    }
}
