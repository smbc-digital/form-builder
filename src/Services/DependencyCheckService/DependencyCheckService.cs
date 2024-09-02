using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using Microsoft.Extensions.Caching.Distributed;
using StockportGovUK.NetStandard.Gateways;
using static Amazon.S3.Util.S3EventNotification;

namespace form_builder.Services.DependencyCheckService
{
    public class DependencyCheckService : IDependencyCheckService
    {
        private readonly IGateway _gateway;

        public DependencyCheckService(
            IGateway gateway
        )
        {
            _gateway = gateway;
        }
        public async Task<bool> IsAvailable(List<string> CheckList)
        {
            //check the dep check serv

            _gateway.ChangeAuthenticationHeader("TestToken");

            var url = "https://localhost:44359/api/v1/Check/";

            var response = await _gateway.PostAsync(url, CheckList); 

            return response.IsSuccessStatusCode;
        }
    }
}
