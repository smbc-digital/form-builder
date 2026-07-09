namespace form_builder.Providers.Lookup;

public class JsonLookupProvider(IGateway gateway) : ILookupProvider
{
    public string ProviderName => "Json";

    public async Task<OptionsResponse> GetAsync(string url, string authToken)
    {
        gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(authToken) ? string.Empty : authToken);

        var response = await gateway.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"JSONLookupProvider::GetAsync, Gateway returned with non success status code of {response.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

        return JsonConvert.DeserializeObject<OptionsResponse>(await response.Content.ReadAsStringAsync());
    }

    public async Task<List<AppointmentType>> GetAppointmentTypesAsync(string url, string authToken)
    {
        gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(authToken) ? string.Empty : authToken);

        var response = await gateway.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"JSONLookupProvider::GetAsync, Gateway returned with non success status code of {response.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

        return JsonConvert.DeserializeObject<List<AppointmentType>>(await response.Content.ReadAsStringAsync());
    }
}