﻿using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Providers.Address
{
    public class CRMStreetProvider : IAddressProvider
    {
        public string ProviderName => "CRMStreet";

        private readonly IVerintServiceGateway _verintServiceGateway;

        public CRMStreetProvider(IVerintServiceGateway verintServiceGateway)
        {
            _verintServiceGateway = verintServiceGateway;
        }

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string street)
        {
            var response = await _verintServiceGateway.GetStreetByReference(street);
            return response.ResponseContent;
        }
    }
}
