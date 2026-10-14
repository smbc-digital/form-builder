namespace form_builder.Providers.Submit;

public class PowerAppsSubmitProvider(IGateway gateway) : ISubmitProvider
{
    public string ProviderName => "flowtoken";

    public async Task<HttpResponseMessage> PostAsync(MappingEntity mappingEntity, SubmitSlug submitSlug) =>
        await gateway.PostAsync(submitSlug.URL, mappingEntity.Data, submitSlug.Type, submitSlug.AuthToken);
}