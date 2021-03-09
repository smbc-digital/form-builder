using form_builder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Providers.DynamicLookupData
{
    public class FakeLookupDataProvider : IDynamicLookDataProvider
    {
        public string ProviderName { get => "Fake"; }

        public async Task<IList<Option>> GetAsync(string key)
        {
            var options = new List<Option>{
                new Option {
                    Text = "Black Container",
                    Value = "black-container",
                    Checked = false
                },
                new Option {
                    Text = "Blue Container",
                    Value = "blue-container",
                    Checked = false
                },
                new Option {
                    Text = "Green Container",
                    Value = "green-container",
                    Checked = false
                },
                new Option {
                    Text = "Brown Container",
                    Value = "brown-container",
                    Checked = false
                }
            };

            return await Task.FromResult(options);
        }
    }
}
