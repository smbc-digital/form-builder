﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Providers.Lookup
{
    public class FakeWasteLookupProvider : ILookupProvider
    {
        public string ProviderName { get => "FakeWaste"; }
        private readonly IGateway _gateway;
        public FakeWasteLookupProvider(IGateway gateway) => _gateway = gateway;

        public async Task<IList<Option>> GetAsync(string url, string authToken)
        {
            _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(authToken) ? string.Empty : authToken);

            var response = await _gateway.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"WasteLookupProvider::GetAsync, Gateway returned with non success status code of {response.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

            return JsonConvert.DeserializeObject<List<Option>>(await response.Content.ReadAsStringAsync());
        }
    }
}