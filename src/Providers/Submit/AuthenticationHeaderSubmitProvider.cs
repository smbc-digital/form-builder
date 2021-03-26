using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace form_builder.Providers.Submit {
    public class AuthenticationHeaderSubmitProvider : ISubmitProvider {
        public string ProviderName => "AuthHeader";
        private IGateway _gateway;

        public AuthenticationHeaderSubmitProvider(IGateway gateway) {
            _gateway = gateway;
        }

        public async Task<HttpResponseMessage> PostAsync(MappingEntity mappingEntity, SubmitSlug submitSlug) {
            _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(submitSlug.AuthToken)
                    ? string.Empty
                    : submitSlug.AuthToken);

            return await _gateway.PostAsync(submitSlug.URL, mappingEntity.Data);
        }
    }
}
