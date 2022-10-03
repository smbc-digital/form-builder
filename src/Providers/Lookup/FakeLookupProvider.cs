using form_builder.Models;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;

namespace form_builder.Providers.Lookup
{
    public class FakeLookupProvider : ILookupProvider
    {
        public string ProviderName { get => "Fake"; }
        public async Task<OptionsResponse> GetAsync(string url, string authToken)
        {
            if (url.Contains("waste", StringComparison.OrdinalIgnoreCase))
                return await Waste();

            return await Generic();
        }

        public async Task<List<AppointmentType>> GetAppointmentTypesAsync(string url, string authToken)
        {
            return await Task.FromResult(new List<AppointmentType>
            {
                new()
                {
                    Environment = "local",
                    AppointmentId = new Guid("6ab97a7b-0ef7-4e4e-958f-937f98d3c2be"),
                    OptionalResources = new List<BookingResource>
                    {
                        new()
                        {
                            ResourceId = new Guid("a4cff58b-096f-4275-8197-35d9d8192aad"),
                            Quantity = 1
                        }
                    }
                }
            });
        }

        private static async Task<OptionsResponse> Waste()
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

            return await Task.FromResult(new OptionsResponse
            {
                Options = options,
                SelectExactly = 1
            });
        }

        private static async Task<OptionsResponse> Generic()
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

            return await Task.FromResult(new OptionsResponse
            {
                Options = options,
                SelectExactly = 1
            });
        }
    }
}