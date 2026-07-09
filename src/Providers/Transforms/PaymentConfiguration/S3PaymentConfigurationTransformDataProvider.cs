namespace form_builder.Providers.Transforms.PaymentConfiguration;

public class S3PaymentConfigurationTransformDataProvider(IS3Gateway s3Gateway,
    IWebHostEnvironment environment,
    IOptions<S3SchemaProviderConfiguration> s3SchemaConfiguration)
    : IPaymentConfigurationTransformDataProvider
{
    private readonly S3SchemaProviderConfiguration _s3SchemaConfiguration = s3SchemaConfiguration.Value;

    public async Task<T> Get<T>()
    {
        try
        {
            var s3Result = await s3Gateway.GetObject(_s3SchemaConfiguration.S3BucketKey, $"{environment.EnvironmentName.ToS3EnvPrefix()}/payment-config/paymentconfiguration.{environment.EnvironmentName}.json");

            using Stream responseStream = s3Result.ResponseStream;
            using StreamReader sr = new(responseStream);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new();
            return serializer.Deserialize<T>(reader);
        }
        catch (AmazonS3Exception e)
        {
            throw new Exception($"S3PaymentConfigurationTransformDataProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {environment.EnvironmentName.ToS3EnvPrefix()}/payment-config/paymentconfiguration.{_environment.EnvironmentName.ToS3EnvPrefix()}.json ", e);
        }
        catch (Exception e)
        {
            throw new Exception($"S3PaymentConfigurationTransformDataProvider: An error has occured while attempting to deserialise object, Exception: {e.Message}", e);
        }
    }
}