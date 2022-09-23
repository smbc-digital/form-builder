using StockportGovUK.NetStandard.Gateways.Models.Addresses;

namespace form_builder.Providers.Street
{
    public class FakeStreetProvider : IStreetProvider
    {
        public string ProviderName { get => "Fake"; }

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string street)
        {
            if (street.ToLower().Replace(" ", "").Equals("nodata"))
                return await Task.FromResult(new List<AddressSearchResult>());

            return await Task.FromResult(new List<AddressSearchResult> {
                new AddressSearchResult {
                    Name = "Green lane",
                    UniqueId = "123456789012"
                },
                 new AddressSearchResult {
                    Name = "Green road",
                    UniqueId = "098765432109"
                },
                 new AddressSearchResult {
                    Name = "Green street",
                    UniqueId = "564737838937"
                }
            });
        }
    }
}