using StockportGovUK.NetStandard.Gateways.Models.Verint.Lookup;

namespace form_builder.Providers.Organisation
{
    public class FakeOrganisationProvider : IOrganisationProvider
    {
        public string ProviderName => "Fake";
        public async Task<IEnumerable<OrganisationSearchResult>> SearchAsync(string organisation)
        {
            if (organisation.ToLower().Replace(" ", "").Equals("nodata"))
                return await Task.FromResult(new List<OrganisationSearchResult>());

            return await Task.FromResult(new List<OrganisationSearchResult> {
                new OrganisationSearchResult {
                    Name = "Organisation 1",
                    Reference = "0101010101"
                },
                new OrganisationSearchResult {
                    Name = "Organisation 2",
                    Reference = "0202020202",
                    Address = "1 street, city, sk11aa"
                },
                new OrganisationSearchResult {
                    Name = "Organisation 3",
                    Reference = "030303030303",
                    Address = "3 town hall lane, city, sk11aa"
                }
            });
        }
    }
}