using Amazon.S3.Model;
using System.Threading.Tasks;

namespace form_builder.Gateway
{
    public interface IS3Gateway
    {
        Task<GetObjectResponse> GetObject(string bucketName, string key);
    }
}
