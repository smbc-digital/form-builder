using System.Threading.Tasks;
using Amazon.S3.Model;

namespace form_builder.Gateways
{
    public interface IS3Gateway
    {
        Task<GetObjectResponse> GetObject(string bucketName, string key);
    }
}
