using System.Threading.Tasks;
using Amazon.S3.Model;

namespace form_builder.Gateways
{
    public interface IS3Gateway
    {
        Task<GetObjectResponse> GetObject(string bucketName, string key);
        Task<ListObjectsV2Response> ListObjectsV2(string bucketName, string prefix);
        Task DeleteObject(string bucketName, string filename);
        Task PutObject(string bucketName, string filename, string imageContent);
    }
}
