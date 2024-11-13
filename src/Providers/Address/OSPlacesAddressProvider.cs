using form_builder.Controllers.Document;
using form_builder.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;

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
            catch (Exception)
            {
                _logger.LogError($"OSPlaces Address provider:: response {response.StatusCode} {response.RequestMessage}");
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

