using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Providers.Address
{
    public class FakeAddressProvider: IAddressProvider
    {
        public string ProviderName { get => "Fake"; }
        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string postcode)
        {
            if(postcode.ToLower().Replace(" ", "").Equals("sk11zz"))
                return await Task.FromResult(new List<AddressSearchResult>());

            return await Task.FromResult(new List<AddressSearchResult> {
                new AddressSearchResult {
                    Name = "address 1",
                    UniqueId = "123456789012"
                },
                 new AddressSearchResult {
                    Name = "address 2",
                    UniqueId = "098765432109"
                },
                 new AddressSearchResult {
                    Name = "address 3",
                    UniqueId = "564737838937"
                }
            });
        }
    }
}