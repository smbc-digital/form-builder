using form_builder.Controllers.Document;
using form_builder.Models;
using Microsoft.DotNet.MSIdentity.Shared;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGet.Common;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace form_builder.Providers.Address
{
    public class OSPlacesAddressProvider : IAddressProvider
    {
        private readonly ILogger<OSPlacesAddressProvider> _logger;
        public string ProviderName => "OSPlaces";

        private readonly IGateway _gateway;
        private readonly OSPlacesAddressProviderConfiguration _oSPlacesAddressProviderConfiguration;

        public OSPlacesAddressProvider(IGateway gateway, IOptions<OSPlacesAddressProviderConfiguration> oSPlacesAddressProviderConfiguration, ILogger<OSPlacesAddressProvider> logger)
        {
            _gateway = gateway;
            _oSPlacesAddressProviderConfiguration = oSPlacesAddressProviderConfiguration.Value;
            _logger = logger;
        }

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
        {
            string clientId = _oSPlacesAddressProviderConfiguration.ClientID;
            string clientSecret = _oSPlacesAddressProviderConfiguration.ClientSecret;

            string url = "https://api.os.uk/oauth2/token/v1";

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

                HttpResponseMessage responseFromAPI = await client.PostAsync(url, content);
                string resultFromAPI = await responseFromAPI.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(resultFromAPI);
                string accessToken = doc.RootElement.GetProperty("access_token").GetString();
                _gateway.ChangeAuthenticationHeader($"Bearer {accessToken}");
            }

            _logger.LogWarning($"OSPlaces Address provider:: about to run the GetAsync: postcode {streetOrPostcode} key {_oSPlacesAddressProviderConfiguration.Key} ");
            string classificationCode;
            if (streetOrPostcode.Contains(":full"))
            {
                classificationCode = string.Empty;
            }
            else
            {
                classificationCode = "&fq=local_custodian_code:" + _oSPlacesAddressProviderConfiguration.LocalCustodianCode;
            }

            string postcode = streetOrPostcode.Replace(":full", "");

            HttpResponseMessage response = null;

            try
            {
                if (streetOrPostcode.Contains(":full"))
                {
                    response = await _gateway.GetAsync($"{_oSPlacesAddressProviderConfiguration.Host}?postcode={postcode}&fq=CLASSIFICATION_CODE:R*%20CLASSIFICATION_CODE:R*%20CLASSIFICATION_CODE:C*&key={_oSPlacesAddressProviderConfiguration.Key}&dataset=LPI");
                }
                else
                {
                    response = await _gateway.GetAsync($"{_oSPlacesAddressProviderConfiguration.Host}?postcode={postcode}&fq=local_custodian_code:{_oSPlacesAddressProviderConfiguration.LocalCustodianCode}&fq=CLASSIFICATION_CODE:R*%20CLASSIFICATION_CODE:R*%20CLASSIFICATION_CODE:C*&key={_oSPlacesAddressProviderConfiguration.Key}&dataset=LPI");
                }
                _logger.LogWarning($"OSPlaces Address provider:: response {response.StatusCode} {response.RequestMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"OSPlaces Address provider:: exception thrown  {ex.Message}");
                throw new ApplicationException($"OSPlacesAddressService::OSPlaces provider Address search, An exception has occurred while attempting to perform postcode lookup with Provider key: '{_oSPlacesAddressProviderConfiguration.Key}' with searchterm '{postcode}' Exception: {ex.Message}", ex);
            }

            var result = await response.Content.ReadAsStringAsync();

            var addresses = JsonConvert.DeserializeObject<OSProperty>(result);

            try
            {
                return addresses.results
                    .Select(address => new AddressSearchResult
                    {
                        AddressLine1 = address.LPI.PAO_START_NUMBER + " " + address.LPI.STREET_DESCRIPTION,
                        AddressLine2 = address.LPI.LOCALITY_NAME,
                        AddressLine3 = address.LPI.TOWN_NAME,
                        Postcode = address.LPI.POSTCODE_LOCATOR,
                        UniqueId = address.LPI.UPRN
                    }).OrderBy(_ => _.UniqueId);
            }
            catch (Exception)
            {
                return new List<AddressSearchResult>();
            }
        }
    }
}

