using form_builder.Models;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Providers.Lookup
{
    public class JsonLookupProvider : ILookupProvider
    {
        public string ProviderName { get => "Json"; }
        private readonly IGateway _gateway;
        public JsonLookupProvider(IGateway gateway) => _gateway = gateway;

        public async Task<OptionsResult> GetAsync(string url, string authToken)
        {
            _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(authToken) ? string.Empty : authToken);

            var response = await _gateway.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"JSONLookupProvider::GetAsync, Gateway returned with non success status code of {response.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

            return JsonConvert.DeserializeObject<OptionsResult>(await response.Content.ReadAsStringAsync());
        }
    }
}