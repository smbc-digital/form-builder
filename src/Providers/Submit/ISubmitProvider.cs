namespace form_builder.Providers.Submit;

public interface ISubmitProvider
{
    string ProviderName { get; }
    Task<HttpResponseMessage> PostAsync(MappingEntity mappingEntity, SubmitSlug submitSlug);
}