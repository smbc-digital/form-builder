namespace form_builder.Providers.Transforms.EmailConfiguration
{
    public interface IEmailConfigurationTransformDataProvider
    {
        Task<T> Get<T>();
    }
}
