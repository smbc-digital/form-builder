using System.Threading.Tasks;

namespace form_builder.Providers.Transforms.ProviderAvailabilityConfiguration
{
    public interface IProviderAvailabilityConfigurationTransformDataProvider
    {
        Task<T> Get<T>();
    }
}
