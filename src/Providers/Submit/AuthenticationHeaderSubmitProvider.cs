namespace form_builder.Providers.Submit;

public class AuthenticationHeaderSubmitProvider(IGateway gateway) : ISubmitProvider
{
    public string ProviderName => "AuthHeader";

    public async Task<HttpResponseMessage> PostAsync(MappingEntity mappingEntity, SubmitSlug submitSlug)
    {
        gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(submitSlug.AuthToken)
            ? string.Empty : submitSlug.AuthToken);

        return await gateway.PostAsync(submitSlug.URL, mappingEntity.Data);
    }
}