using System.Threading.Tasks;
using form_builder.Providers.TransformDataProvider;

namespace form_builder.Providers.SchemaProvider
{
    public class S3TransformDataProvider : ITransformDataProvider
    {
        public Task<T> Get<T>(string schemaName)
        {
            throw new System.NotImplementedException();
        }
    }
}