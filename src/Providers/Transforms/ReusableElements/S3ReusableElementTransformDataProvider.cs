namespace form_builder.Providers.Transforms.ReusableElements;

public class S3ReusableElementTransformDataProvider(IS3Gateway s3Gateway,
    IWebHostEnvironment environment,
    IOptions<S3SchemaProviderConfiguration> s3SchemaConfiguration) : IReusableElementTransformDataProvider
{
    private readonly S3SchemaProviderConfiguration _s3SchemaConfiguration = s3SchemaConfiguration.Value;

    public async Task<IElement> Get(string schemaName)
    {
        try
        {
            var s3Result = await s3Gateway.GetObject(_s3SchemaConfiguration.S3BucketKey, $"{environment.EnvironmentName.ToS3EnvPrefix()}/Elements/{schemaName}.json");

            using Stream responseStream = s3Result.ResponseStream;
            using StreamReader sr = new(responseStream);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new();
            return serializer.Deserialize<IElement>(reader);
        }
        catch (AmazonS3Exception e)
        {
            throw new Exception($"S3ReusableElementTransformDataProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {environment.EnvironmentName.ToS3EnvPrefix()}/Elements/{schemaName} ", e);
        }
        catch (Exception e)
        {
            throw new Exception($"S3ReusableElementTransformDataProvider: An error has occured while attempting to deserialise object, Exception: {e.Message}", e);
        }
    }
}