using Amazon.S3;
using form_builder.Gateways;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace form_builder.Providers.FileStorage
{
    public class S3FileStorageProvider : IFileStorageProvider
    {
        public string ProviderName { get => "S3"; }
        private readonly IS3Gateway _s3Gateway;
        private readonly IConfiguration _configuration;

        public S3FileStorageProvider(IS3Gateway s3Service, IConfiguration configuration)
        {
            _s3Gateway = s3Service;
            _configuration = configuration;
        }
        public async Task<string> GetString(string key)
        {
            try
            {
                var s3Response = await _s3Gateway.GetObject(_configuration["FileStorageProvider:S3BucketName"], key);
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

        public async Task Remove(string filename)
        {
            try
            {
                await _s3Gateway.DeleteObject(_configuration["FileStorageProvider:S3BucketName"], filename);
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception($"S3FileStorageProvider: An error has occured while attempting to delete object in s3 bucket, Exception: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"S3FileStorageProvider: An error has occured while attempting to delete object, Exception: {e.Message}", e);
            }
        }

        public async Task SetStringAsync(string filename, string value, int expiration, CancellationToken token = default)
        {
            try
            {
                await _s3Gateway.PutObject(_configuration["FileStorageProvider:S3BucketName"], filename, value);
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception($"S3FileStorageProvider: An error has occured while attempting to put object in s3 bucket, Exception: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"S3FileStorageProvider: An error has occured while attempting to put object, Exception: {e.Message}", e);
            }
        }
    }
}
