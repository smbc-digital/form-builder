using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace form_builder.Extensions {
    public static class GatewayExtentions {

        public async static Task<HttpResponseMessage> PostAsync(this IGateway gateway, MappingEntity mappingEntity, SubmitSlug submitSlug) {
            switch (submitSlug.Type) {
                case "flowtoken":
                    return await gateway.PostAsync(submitSlug.URL, mappingEntity.Data, submitSlug.Type, submitSlug.AuthToken);
                default:
                    gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(submitSlug.AuthToken)
                    ? string.Empty
                    : submitSlug.AuthToken);

                    return await gateway.PostAsync(submitSlug.URL, mappingEntity.Data);
            }
        }
    }
}
