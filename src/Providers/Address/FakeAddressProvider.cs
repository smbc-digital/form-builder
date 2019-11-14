using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Providers.Address
{
    public class FakeAddressProvider: IAddressProvider
    {

        public string ProviderName { get => "Fake"; }
        public async Task<ICollection<AddressSearchResult>> SearchAsync(string postcode)
        {
            return new List<AddressSearchResult> {
                new AddressSearchResult {
                    Description = "address 1",
                    Uprn = "111111"
                },
                 new AddressSearchResult {
                    Description = "address 2",
                    Uprn = "222222"
                },
                 new AddressSearchResult {
                    Description = "address 3",
                    Uprn = "333333"
                }
            };
        }

        // public AddressSearchResult Search(string postcode)
        // {

        // }

        // public AddressDetails GetAddressDetails(string id)
        // {

        // }
    }
}