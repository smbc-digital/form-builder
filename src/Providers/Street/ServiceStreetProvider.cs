namespace form_builder.Providers.Street;

public class ServiceStreetProvider(IStreetServiceGateway streetServiceGateway) : IStreetProvider
{
    public string ProviderName => "CRM";

    public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string street)
    {
        var response = await streetServiceGateway.SearchAsync(new StreetSearch { StreetProvider = EStreetProvider.CRM, SearchTerm = street });
        return response.ResponseContent;
    }
}