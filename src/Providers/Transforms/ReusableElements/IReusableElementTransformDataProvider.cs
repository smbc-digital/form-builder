using System.Threading.Tasks;

namespace form_builder.Providers.Transforms.ReusableElements
{
    public interface IReusableElementTransformDataProvider
    {
        Task<T> Get<T>(string schemaName);
    }
}