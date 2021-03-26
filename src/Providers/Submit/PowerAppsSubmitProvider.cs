using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace form_builder.Providers.Submit {
    public class PowerAppsSubmitProvider : ISubmitProvider {
        public string ProviderName => "flowtoken";
        private IGateway _gateway;

        public PowerAppsSubmitProvider(IGateway gateway) {
            _gateway = gateway;
        }

        public async Task<HttpResponseMessage> PostAsync(MappingEntity mappingEntity, SubmitSlug submitSlug) {
            return await _gateway.PostAsync(submitSlug.URL, mappingEntity.Data, submitSlug.Type, submitSlug.AuthToken);
        }
    }
}
