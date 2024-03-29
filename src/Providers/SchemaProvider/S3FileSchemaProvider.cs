﻿using Amazon.S3;
using Amazon.S3.Model;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Extensions;
using form_builder.Gateways;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Providers.SchemaProvider
{
    public class S3FileSchemaProvider : ISchemaProvider
    {
        private readonly IS3Gateway _s3Gateway;
        private readonly IWebHostEnvironment _environment;
        private readonly S3SchemaProviderConfiguration _s3SchemaConfiguration;
        private readonly ILogger<ISchemaProvider> _logger;
        private readonly IDistributedCacheWrapper _distributedCacheWrapper;
        private readonly DistributedCacheConfiguration _distributedCacheConfiguration;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public S3FileSchemaProvider(IS3Gateway s3Service,
            IWebHostEnvironment environment,
            IDistributedCacheWrapper distributedCacheWrapper,
            IOptions<S3SchemaProviderConfiguration> s3SchemaConfiguration,
            IOptions<DistributedCacheConfiguration> distributedCacheConfiguration,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            ILogger<ISchemaProvider> logger)
        {
            _s3Gateway = s3Service;
            _environment = environment;
            _s3SchemaConfiguration = s3SchemaConfiguration.Value;
            _logger = logger;
            _distributedCacheWrapper = distributedCacheWrapper;
            _distributedCacheConfiguration = distributedCacheConfiguration.Value;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        }

        public async Task<T> Get<T>(string schemaName)
        {
            try
            {
                var s3Result = await _s3Gateway.GetObject(_s3SchemaConfiguration.S3BucketKey, $"{_environment.EnvironmentName.ToS3EnvPrefix()}/{_s3SchemaConfiguration.S3BucketFolderName}/{schemaName}.json");

                using Stream responseStream = s3Result.ResponseStream;
                using StreamReader sr = new(responseStream);
                using JsonReader reader = new JsonTextReader(sr);
                JsonSerializer serializer = new();
                return serializer.Deserialize<T>(reader);
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception($"S3FileSchemaProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {_environment.EnvironmentName.ToS3EnvPrefix()}/{_s3SchemaConfiguration.S3BucketKey}/{_s3SchemaConfiguration.S3BucketFolderName}/{schemaName}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"S3FileSchemaProvider: An error has occured while attempting to deserialize object, Exception: {e.Message}", e);
            }
        }

        public FormSchema Get(string schemaName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> IndexSchema()
        {
            if (!_distributedCacheConfiguration.UseDistributedCache)
            {
                _logger.LogWarning($"S3FileSchemaProvider::IndexSchema, A request to index form schema was made but UseDistributedCache is disabled");
                return new List<string>();
            }

            ListObjectsV2Response result = new();
            string bucketSearchPrefix = $"{_environment.EnvironmentName.ToS3EnvPrefix()}/{_s3SchemaConfiguration.S3BucketFolderName}/";
            try
            {
                result = await _s3Gateway.ListObjectsV2(_s3SchemaConfiguration.S3BucketKey, bucketSearchPrefix);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"S3FileSchemaProvider::IndexSchema, Failed to retrieve list of forms from s3 bucket {bucketSearchPrefix}, Exception: {e.Message}");
                return new List<string>();
            }

            var indexKeys = result.S3Objects.Select(_ => _.Key.Remove(0, bucketSearchPrefix.Length)).ToList();

            _ = _distributedCacheWrapper.SetStringAsync(CacheConstants.INDEX_SCHEMA, JsonConvert.SerializeObject(indexKeys), _distributedCacheExpirationConfiguration.Index);

            return indexKeys;
        }

        public async Task<bool> ValidateSchemaName(string schemaName)
        {
            if (!_distributedCacheConfiguration.UseDistributedCache)
                return true;

            var cachedIndexSchema = _distributedCacheWrapper.GetString(CacheConstants.INDEX_SCHEMA);
            var indexSchema = new List<string>();

            if (string.IsNullOrEmpty(cachedIndexSchema))
                indexSchema = await IndexSchema();
            else
                indexSchema = JsonConvert.DeserializeObject<List<string>>(cachedIndexSchema);

            return indexSchema.Any(_ => _.Equals($"{schemaName}.json", StringComparison.OrdinalIgnoreCase));
        }
    }
}
