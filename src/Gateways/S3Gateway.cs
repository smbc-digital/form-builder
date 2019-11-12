using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace form_builder.Gateways
{
    public class S3Gateway : IS3Gateway
    {
        private readonly IAmazonS3 _s3client;

        public S3Gateway(IAmazonS3 s3client)
        {
            _s3client = s3client;
        }

        public async Task<GetObjectResponse> GetObject(string bucketName, string key)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            return await _s3client.GetObjectAsync(getRequest);
        }
    }
}
