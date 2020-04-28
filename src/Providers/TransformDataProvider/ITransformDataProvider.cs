using System.Threading.Tasks;

namespace form_builder.Providers.TransformDataProvider
{
    public interface ITransformDataProvider
    {
        Task<T> Get<T>(string schemaName);
    }
}