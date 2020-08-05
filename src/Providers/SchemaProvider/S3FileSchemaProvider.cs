using form_builder.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Hosting;
using form_builder.Extensions;
using form_builder.Gateways;
using Microsoft.Extensions.Configuration;

namespace form_builder.Providers.SchemaProvider
{
    public class S3FileSchemaProvider : ISchemaProvider
    {
        private readonly IS3Gateway _s3Gateway;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public S3FileSchemaProvider(IS3Gateway s3Service, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _s3Gateway = s3Service;
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<T> Get<T>(string schemaName)
        {
            try
            {
                var s3Result = await _s3Gateway.GetObject(_configuration["S3BucketKey"], $"{_environment.EnvironmentName.ToS3EnvPrefix()}/{schemaName}.json");

                using (Stream responseStream = s3Result.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    var responseBody = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }
            }
            catch (AmazonS3Exception e)
            {
                var ex = new Exception($"S3FileSchemaProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}. {_environment.EnvironmentName.ToS3EnvPrefix()} {schemaName} ", e);
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
    }
}
