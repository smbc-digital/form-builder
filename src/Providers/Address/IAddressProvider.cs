using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Providers.Address
{
    public interface IAddressProvider
    {

        string ProviderName { get; }

        Task<ICollection<AddressSearchResult>> SearchAsync(string postcode);

        //AddressSearchResult Search(string postcode);

        //AddressDetails GetAddressDetails(string id);
    }
}