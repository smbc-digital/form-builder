﻿using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace form_builder.Gateways
{
    public class S3Gateway : IS3Gateway
    {
        private readonly IAmazonS3 _s3client;

        public S3Gateway(IAmazonS3 s3client) => _s3client = s3client;

        public async Task<GetObjectResponse> GetObject(string bucketName, string key)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            return await _s3client.GetObjectAsync(getRequest);
        }

        public async Task<ListObjectsV2Response> ListObjectsV2(string bucketName, string prefix)
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            };

            return await _s3client.ListObjectsV2Async(listRequest);
        }

        public async Task DeleteObject(string bucketName, string filename) =>
            await _s3client.DeleteObjectAsync(bucketName, filename);

        public async Task PutObject(string bucketName, string filename, string imageContent)
        {
            var fileTransferUtility = new TransferUtility(_s3client);
            using var ms = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(imageContent));
            await fileTransferUtility.UploadAsync(ms, bucketName, filename);
        }
    }
}
