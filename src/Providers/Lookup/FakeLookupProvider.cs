using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Providers.Lookup
{
    public class FakeLookupProvider : ILookupProvider
    {
        public async Task<IList<Option>> GetAsync(string key)
        {
            var options = new List<Option>{
                new Option {
                    Text = "Yes",
                    Value = "True",
                    Checked = false
                },
                new Option {
                    Text = "No",
                    Value = "False",
                    Checked = false
                },
                new Option {
                    Text = "Maybe",
                    Value = "Maybe",
                    Checked = false
                },
                new Option {
                    Text = "Can you repeat the question?",
                    Value = "False",
                    Checked = false
                }
            };

            return await Task.FromResult(options);
        }
    }
}