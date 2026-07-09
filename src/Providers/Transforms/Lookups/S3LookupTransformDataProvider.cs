namespace form_builder.Providers.Transforms.Lookups;

public class S3LookupTransformDataProvider(IS3Gateway s3Gateway,
    IWebHostEnvironment environment,
    IOptions<S3SchemaProviderConfiguration> s3SchemaConfiguration)
    : ILookupTransformDataProvider
{
    private readonly S3SchemaProviderConfiguration _s3SchemaConfiguration = s3SchemaConfiguration.Value;

    public async Task<T> Get<T>(string schemaName)
    {
        try
        {
            var s3Result = await s3Gateway.GetObject(_s3SchemaConfiguration.S3BucketKey, $"{environment.EnvironmentName.ToS3EnvPrefix()}/Lookups/{schemaName}.json");

            using Stream responseStream = s3Result.ResponseStream;
            using StreamReader sr = new(responseStream);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new();
            return serializer.Deserialize<T>(reader);
        }
        catch (AmazonS3Exception e)
        {
            throw new Exception($"S3LookupTransformDataProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {environment.EnvironmentName.ToS3EnvPrefix()}/Lookups/{schemaName} ", e);
        }
        catch (Exception e)
        {
            throw new Exception($"S3LookupTransformDataProvider: An error has occured while attempting to deserialise object, Exception: {e.Message}", e);
        }
    }
}