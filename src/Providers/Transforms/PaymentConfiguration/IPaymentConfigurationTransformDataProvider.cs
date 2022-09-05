namespace form_builder.Providers.Transforms.PaymentConfiguration
{
    public interface IPaymentConfigurationTransformDataProvider
    {
        Task<T> Get<T>();
    }
}
