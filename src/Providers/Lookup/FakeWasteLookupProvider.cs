using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Properties.ElementProperties;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Providers.Lookup
{
    public class FakeWasteLookupProvider : ILookupProvider
    {
        public string ProviderName { get => "FakeWaste"; }
        private readonly IGateway _gateway;
        private readonly IWebHostEnvironment _environment;
        public FakeWasteLookupProvider(IGateway gateway, IWebHostEnvironment enviroment)
        {
            _gateway = gateway;
            _environment = enviroment;
        }

        public async Task<IList<Option>> GetAsync(form_builder.Models.Properties.ElementProperties.Lookup lookup, string query)
        {
            var submitDetails = lookup.Environments.SingleOrDefault(x => x.Environment.Equals(_environment.EnvironmentName));

            _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(submitDetails.AuthToken) ? string.Empty : submitDetails.AuthToken);

            submitDetails.URL += query;
            var response = await _gateway.GetAsync(submitDetails.URL);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"WasteLookupProvider::GetAsync, Gateway returned with non success status code of {response.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

            // Handle success
            var content = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<Option>>(content);

            return results;
        }
    }
}