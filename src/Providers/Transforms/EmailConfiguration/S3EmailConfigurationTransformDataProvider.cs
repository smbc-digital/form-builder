using Amazon.S3;
using form_builder.Configuration;
using form_builder.Extensions;
using form_builder.Gateways;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.EmailConfiguration
{
    public class S3EmailConfigurationTransformDataProvider : IEmailConfigurationTransformDataProvider
    {
        private readonly IS3Gateway _s3Gateway;
        private readonly IWebHostEnvironment _environment;
        private readonly S3SchemaProviderConfiguration _s3SchemaConfiguration;

        public S3EmailConfigurationTransformDataProvider(IS3Gateway s3Service, IWebHostEnvironment environment, IOptions<S3SchemaProviderConfiguration> S3SchemaConfiguration)
        {
            _s3Gateway = s3Service;
            _environment = environment;
            _s3SchemaConfiguration = S3SchemaConfiguration.Value;
        }

        public async Task<T> Get<T>()
        {
            try
            {
                var s3Result = await _s3Gateway.GetObject(_s3SchemaConfiguration.S3BucketKey, $"{_environment.EnvironmentName.ToS3EnvPrefix()}/email-config/emailconfiguration.{_environment.EnvironmentName}.json");

                using Stream responseStream = s3Result.ResponseStream;
                using StreamReader sr = new(responseStream);
                using JsonReader reader = new JsonTextReader(sr);
                JsonSerializer serializer = new();
                return serializer.Deserialize<T>(reader);
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception($"S3EmailConfigurationTransformDataProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {_environment.EnvironmentName.ToS3EnvPrefix()}/email-config/emailconfiguration.{_environment.EnvironmentName.ToS3EnvPrefix()}.json ", e);
            }
            catch (Exception e)
            {
                throw new Exception($"S3EmailConfigurationTransformDataProvider: An error has occured while attempting to deserialise object, Exception: {e.Message}", e);
            }
        }
    }
}
