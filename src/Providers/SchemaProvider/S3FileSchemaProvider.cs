using form_builder.Gateway;
using form_builder.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using form_builder.Extensions;

namespace form_builder.Providers.SchemaProvider
{
    public class S3FileSchemaProvider : ISchemaProvider
    {
        private readonly IS3Gateway _s3Gateway;
        private readonly ILogger<S3FileSchemaProvider> _logger;
        private readonly IHostingEnvironment _enviroment;

        public S3FileSchemaProvider(IS3Gateway s3Service, ILogger<S3FileSchemaProvider> logger, IHostingEnvironment enviroment)
        {
            _s3Gateway = s3Service;
            _logger = logger;
            _enviroment = enviroment;
        }

        public async Task<T> Get<T>(string schemaName)
        {
            try
            {
                var s3Result = await _s3Gateway.GetObject("form-json", $"{_enviroment.EnvironmentName.ToS3EnvPrefix()}/{schemaName}.json");

                using (Stream responseStream = s3Result.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    var responseBody = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError($"S3FileSchemaProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"S3FileSchemaProvider: An error has occured while attempting to deserialize object, Exception: {e.Message}");
                throw;
            }
        }

        public FormSchema Get(string schemaName)
        {
            throw new NotImplementedException();
        }
    }
}
