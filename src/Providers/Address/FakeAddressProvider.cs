using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Providers.Address
{
    public class FakeAddressProvider: IAddressProvider
    {

        public string ProviderName { get => "Fake"; }
        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string postcode)
        {
            return new List<AddressSearchResult> {
                new AddressSearchResult {
                    Name = "address 1",
                    UniqueId = "111111"
                },
                 new AddressSearchResult {
                    Name = "address 2",
                    UniqueId = "222222"
                },
                 new AddressSearchResult {
                    Name = "address 3",
                    UniqueId = "333333"
                }
            };
        }
    }
}