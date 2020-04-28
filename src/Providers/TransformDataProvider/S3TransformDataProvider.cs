using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using form_builder.Extensions;
using form_builder.Gateways;
using form_builder.Providers.TransformDataProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace form_builder.Providers.SchemaProvider
{
    public class S3TransformDataProvider : ITransformDataProvider
    {
        private readonly IS3Gateway _s3Gateway;
        private readonly ILogger<S3TransformDataProvider> _logger;
        private readonly IHostingEnvironment _enviroment;
        private readonly IConfiguration _configuration;

        public S3TransformDataProvider(IS3Gateway s3Service, ILogger<S3TransformDataProvider> logger, IHostingEnvironment enviroment, IConfiguration configuration)
        {
            _s3Gateway = s3Service;
            _logger = logger;
            _enviroment = enviroment;
            _configuration = configuration;
        }

        public async Task<T> Get<T>(string schemaName)
        {
            try
            {
                var s3Result = await _s3Gateway.GetObject(_configuration["S3BucketKey"], $"{_enviroment.EnvironmentName.ToS3EnvPrefix()}/Lookups/{schemaName}.json");

                using (Stream responseStream = s3Result.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    var responseBody = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }
            }
            catch (AmazonS3Exception e)
            {
                var ex = new Exception($"S3TransformDataProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {_enviroment.EnvironmentName.ToS3EnvPrefix()}/Lookups/{schemaName} ", e);
                throw ex;
            }
            catch (Exception e)
            {
                var ex = new Exception($"S3TransformDataProvider: An error has occured while attempting to deserialise object, Exception: {e.Message}", e);
                throw ex;
            }
        }
    }
}