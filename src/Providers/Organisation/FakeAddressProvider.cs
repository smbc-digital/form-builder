using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.Organisation
{
    public class FakeOrganisationProvider : IOrganisationProvider
    {
        public string ProviderName => "Fake";
        public async Task<IEnumerable<StockportGovUK.NetStandard.Models.Models.Verint.Organisation>> SearchAsync(string postcode)
        {
            return new List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation> {
                new StockportGovUK.NetStandard.Models.Models.Verint.Organisation {
                    Description = "Organisation 1",
                    Reference = "0101010101"
                },
                new StockportGovUK.NetStandard.Models.Models.Verint.Organisation {
                    Description = "Organisation 2",
                    Reference = "0202020202"
                },
                new StockportGovUK.NetStandard.Models.Models.Verint.Organisation {
                    Description = "Organisation 3",
                    Reference = "030303030303"
                }
            };
        }
    }
}