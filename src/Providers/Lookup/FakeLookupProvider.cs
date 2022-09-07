using form_builder.Models;

namespace form_builder.Providers.Lookup
{
    public class FakeLookupProvider : ILookupProvider
    {
        public string ProviderName { get => "Fake"; }
        public async Task<List<Option>> GetAsync(string url, string authToken)
        {
            if (url.Contains("waste", StringComparison.OrdinalIgnoreCase))
                return await Waste();

            return await Generic();
        }

        private static async Task<List<Option>> Waste()
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

        private static async Task<List<Option>> Generic()
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