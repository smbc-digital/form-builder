using form_builder.Enum;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;

namespace form_builder.ViewModels
{
    public class AddressViewModel
    {
        public List<AddressSearchResult> AddressSearchResults { get; set; }

        public EAddressJourney AddressStatus { get; set; }
    }
}
