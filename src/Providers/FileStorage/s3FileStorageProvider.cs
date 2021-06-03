using Amazon.S3;
using form_builder.Gateways;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace form_builder.Providers.FileStorage
{
    public class s3FileStorageProvider : IFileStorageProvider
    {
        public string ProviderName { get => "s3"; }
        private readonly IS3Gateway _s3Gateway;
        private readonly IConfiguration _configuration;

        public s3FileStorageProvider(IS3Gateway s3Service,
            IConfiguration configuration)
        {
            _s3Gateway = s3Service;
            _configuration = configuration;
        }
        public async Task<string> GetString(string key)
        {
            try
            {
                var s3Response = await _s3Gateway.GetObject(_configuration["FileStorageProvider:Type"], key);
                var s3StringResponse = new StreamReader(s3Response.ResponseStream).ReadToEnd();
                return s3StringResponse;
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception($"S3FileStorageProvider: An error has occured while attempting to get S3 Object, Exception: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"S3FileStorageProvider: An error has occured while attempting to get object, Exception: {e.Message}", e);
            }
        }

        public Task Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task SetStringAsync(string key, string value, int expiration, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
