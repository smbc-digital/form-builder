using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Providers.Street
{
    public class FakeStreetProvider : IStreetProvider
    {
        public string ProviderName { get => "FakeStreet"; }
        public async Task<IEnumerable<StockportGovUK.NetStandard.Models.Models.Verint.Street>> SearchAsync(string street)
        {
            return new List<StockportGovUK.NetStandard.Models.Models.Verint.Street> {
                new StockportGovUK.NetStandard.Models.Models.Verint.Street {
                    Description = "Green lane",
                    USRN = "123456789012"
                },
                 new StockportGovUK.NetStandard.Models.Models.Verint.Street {
                    Description = "Green road",
                    USRN = "098765432109"
                },
                 new StockportGovUK.NetStandard.Models.Models.Verint.Street {
                    Description = "Green street",
                    USRN = "564737838937"
                }
            };
        }
    }
}