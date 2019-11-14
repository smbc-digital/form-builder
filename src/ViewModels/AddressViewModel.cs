using form_builder.Enum;
using form_builder.Models;
using System.Collections.Generic;

namespace form_builder.ViewModels
{
    public class AddressViewModel
    {
        public List<AddressSearchResult> AddressSearchResults { get; set; }

        public EAddressJourney AddressStatus { get; set; }
    }
}
