using form_builder.Models;
using StockportGovUK.NetStandard.Gateways;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Helpers.Session;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Threading;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Services.MappingService;

namespace form_builder.Services.RetrieveExternalDataService
{
    public class RetrieveExternalDataService : IRetrieveExternalDataService
    {
        private readonly IGateway _gateway;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IMappingService _mappingService;
        private readonly IActionsHelper _actionsHelper;

        public RetrieveExternalDataService(IGateway gateway, ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache, IMappingService mappingService, IActionsHelper actionsHelper)
        {
            _gateway = gateway;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
            _mappingService = mappingService;
            _actionsHelper = actionsHelper;
        }
 
        public async Task Process(List<PageAction> actions, string formName)
        {
            var answers = new List<Answers>();
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var mappingData = await _mappingService.Map(sessionGuid, formName);

            foreach (var action in actions)
            {
                var response = new HttpResponseMessage();
                var entity = _actionsHelper.GenerateUrl(action.Properties.URL, mappingData.FormAnswers);

                if (!string.IsNullOrEmpty(action.Properties.AuthToken))
                    _gateway.ChangeAuthenticationHeader(action.Properties.AuthToken);

                if (entity.IsPost)
                {
                    response = await _gateway.PostAsync(entity.Url, mappingData.Data);
                }
                else
                {
                    response = await _gateway.GetAsync(entity.Url);
                }

                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"RetrieveExternalDataService::Process, http request to {entity.Url} returned an unsuccessful status code, Response: {JsonConvert.SerializeObject(response)}");

                if (response.Content == null)
                    throw new ApplicationException($"RetrieveExternalDataService::Process, response content from {entity.Url} is null.");

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    throw new ApplicationException($"RetrieveExternalDataService::Process, Gateway {entity.Url} responded with empty reference");

                answers.Add(new Answers
                {
                    QuestionId = action.Properties.TargetQuestionId,
                    Response = JsonConvert.DeserializeObject<string>(content)
                });
            }

            mappingData.FormAnswers.Pages.FirstOrDefault(_ => _.PageSlug.ToLower().Equals(mappingData.FormAnswers.Path.ToLower())).Answers.AddRange(answers);

            await _distributedCache.SetStringAsync(sessionGuid, JsonConvert.SerializeObject(mappingData.FormAnswers), CancellationToken.None);
        }
    }
}