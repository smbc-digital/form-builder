using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using form_builder.Extensions;
using form_builder.Gateways;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.Lookups
{
    public class S3LookupTransformDataProvider : ILookupTransformDataProvider
    {
        private readonly IS3Gateway _s3Gateway;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public S3LookupTransformDataProvider(IS3Gateway s3Service, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _s3Gateway = s3Service;
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<T> Get<T>(string schemaName)
        {
            try
            {
                var s3Result = await _s3Gateway.GetObject(_configuration["S3BucketKey"], $"{_environment.EnvironmentName.ToS3EnvPrefix()}/Lookups/{schemaName}.json");

                using (Stream responseStream = s3Result.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    var responseBody = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception($"S3LookupTransformDataProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {_environment.EnvironmentName.ToS3EnvPrefix()}/Lookups/{schemaName} ", e);
            }
            catch (Exception e)
            {
                throw new Exception($"S3LookupTransformDataProvider: An error has occured while attempting to deserialise object, Exception: {e.Message}", e);
            }
        }
    }
}