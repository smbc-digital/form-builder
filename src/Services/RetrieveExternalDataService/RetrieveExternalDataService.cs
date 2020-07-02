using form_builder.Models;
using StockportGovUK.NetStandard.Gateways;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using form_builder.Helpers.Session;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Threading;
using form_builder.Services.MappingService;
using form_builder.Services.RetrieveExternalDataService.Entities;

namespace form_builder.Services.RetrieveExternalDataService
{
    public class RetrieveExternalDataService : IRetrieveExternalDataService
    {
        private readonly IGateway _gateway;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IMappingService _mappingService;
        private Regex _tagRegex => new Regex("(?<={{).*?(?=}})", RegexOptions.Compiled);

        public RetrieveExternalDataService(IGateway gateway, ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache, IMappingService mappingService)
        {
            _gateway = gateway;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
            _mappingService = mappingService;
        }
 
        public async Task Process(List<PageAction> actions, string formName)
        {
            var answers = new List<Answers>();
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var mappingData = await _mappingService.Map(sessionGuid, formName);

            foreach (var action in actions)
            {
                var response = new HttpResponseMessage();
                var entity = GenerateUrl(action.Properties.URL, mappingData.FormAnswers);

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
                    Response = content
                });
            }

            mappingData.FormAnswers.Pages.FirstOrDefault(_ => _.PageSlug.ToLower().Equals(mappingData.FormAnswers.Path.ToLower())).Answers.AddRange(answers);

            await _distributedCache.SetStringAsync(sessionGuid, JsonConvert.SerializeObject(mappingData.FormAnswers), CancellationToken.None);
        }

        private ExternalDataEntity GenerateUrl(string baseUrl, FormAnswers formAnswers)
        {
            var matches = _tagRegex.Matches(baseUrl);
            var newUrl = matches.Aggregate(baseUrl, (current, match) => Replace(match, current, formAnswers));
            return new ExternalDataEntity
            {
                Url = newUrl,
                IsPost = !matches.Any()
            };
        }

        private string Replace(Match match, string current, FormAnswers formAnswers)
        {
            var splitTargets = match.Value.Split(".");
            var answer = RecursiveGetAnswerValue(match.Value, formAnswers.Pages.SelectMany(_ => _.Answers).First(a => a.QuestionId.Equals(splitTargets[0])));

            return current.Replace($"{{{{{match.Groups[0].Value}}}}}", answer);
        }

        private string RecursiveGetAnswerValue(string targetMapping, Answers answer)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length == 1)
                return (dynamic)answer.Response;

            var subObject = new Answers{ QuestionId = splitTargets[1], Response = (dynamic)answer.Response[splitTargets[1]] };
            return RecursiveGetAnswerValue(targetMapping.Replace($"{splitTargets[0]}.", string.Empty), subObject);
        }
    }
}