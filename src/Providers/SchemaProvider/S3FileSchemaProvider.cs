namespace form_builder.Providers.SchemaProvider;

public class S3FileSchemaProvider(IS3Gateway s3Gateway,
    IWebHostEnvironment environment,
    IDistributedCacheWrapper distributedCacheWrapper,
    IOptions<S3SchemaProviderConfiguration> s3SchemaConfiguration,
    IOptions<DistributedCacheConfiguration> distributedCacheConfiguration,
    IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
    ILogger<ISchemaProvider> logger)
    : ISchemaProvider
{
    private readonly S3SchemaProviderConfiguration _s3SchemaConfiguration = s3SchemaConfiguration.Value;
    private readonly DistributedCacheConfiguration _distributedCacheConfiguration = distributedCacheConfiguration.Value;
    private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;

    public async Task<T> Get<T>(string schemaName)
    {
        try
        {
            var s3Result = await s3Gateway.GetObject(_s3SchemaConfiguration.S3BucketKey, $"{environment.EnvironmentName.ToS3EnvPrefix()}/{_s3SchemaConfiguration.S3BucketFolderName}/{schemaName}.json");

            using Stream responseStream = s3Result.ResponseStream;
            using StreamReader sr = new(responseStream);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new();
            return serializer.Deserialize<T>(reader);
        }
        catch (AmazonS3Exception e)
        {
            throw new Exception($"S3FileSchemaProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {environment.EnvironmentName.ToS3EnvPrefix()}/{_s3SchemaConfiguration.S3BucketKey}/{_s3SchemaConfiguration.S3BucketFolderName}/{schemaName}", e);
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
            logger.LogWarning($"S3FileSchemaProvider::IndexSchema, A request to index form schema was made but UseDistributedCache is disabled");
            return new List<string>();
        }

        ListObjectsV2Response result = new();
        string bucketSearchPrefix = $"{environment.EnvironmentName.ToS3EnvPrefix()}/{_s3SchemaConfiguration.S3BucketFolderName}/";
        try
        {
            result = await s3Gateway.ListObjectsV2(_s3SchemaConfiguration.S3BucketKey, bucketSearchPrefix);
        }
        catch (Exception e)
        {
            logger.LogWarning($"S3FileSchemaProvider::IndexSchema, Failed to retrieve list of forms from s3 bucket {bucketSearchPrefix}, Exception: {e.Message}");
            return new List<string>();
        }

        var indexKeys = result.S3Objects.Select(_ => _.Key.Remove(0, bucketSearchPrefix.Length)).ToList();

        _ = distributedCacheWrapper.SetStringAsync(CacheConstants.INDEX_SCHEMA, JsonConvert.SerializeObject(indexKeys), _distributedCacheExpirationConfiguration.Index);

        return indexKeys;
    }

    public async Task<bool> ValidateSchemaName(string schemaName)
    {
        if (!_distributedCacheConfiguration.UseDistributedCache)
            return true;

        var cachedIndexSchema = distributedCacheWrapper.GetString(CacheConstants.INDEX_SCHEMA);
        var indexSchema = new List<string>();

        if (string.IsNullOrEmpty(cachedIndexSchema))
            indexSchema = await IndexSchema();
        else
            indexSchema = JsonConvert.DeserializeObject<List<string>>(cachedIndexSchema);

        return indexSchema.Any(_ => _.Equals($"{schemaName}.json", StringComparison.OrdinalIgnoreCase));
    }
}