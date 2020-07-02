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

namespace form_builder.Services.RetrieveExternalDataService
{
    public class RetrieveExternalDataService : IRetrieveExternalDataService
    {
        private readonly IGateway _gateway;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private Regex _tagRegex => new Regex("(?<={{).*?(?=}})", RegexOptions.Compiled);

        public RetrieveExternalDataService(IGateway gateway, ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache)
        {
            _gateway = gateway;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
        }
 
        public Task Process(List<PageAction> actions)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var formAnswers = _distributedCache.GetString(sessionGuid);
            var convertedFormAnswers = JsonConvert.DeserializeObject<FormAnswers>(formAnswers);

            actions.ForEach(async (action) =>
            {
                var response = new HttpResponseMessage();
                var entity = GenerateUrl(action.Properties.URL, convertedFormAnswers);
                _gateway.ChangeAuthenticationHeader("GET-FROM-CONFIG");

                if (entity.IsPost)
                {
                    response = await _gateway.PostAsync(entity.Url, new { } );
                } else {
                    response = await _gateway.GetAsync(entity.Url);
                }

                if(response.IsSuccessStatusCode)
                    throw new ApplicationException($"RetrieveExternalDataService::Process, http request to {entity.Url} returned an unsuccessful status code, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

            });
            //loop over actions
            //identity httpmethod
            //sort out authToken
            //genertae the url
            //perform action
            //save to answers
            throw new System.NotImplementedException();
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

            //find {{}}
            //get the value and replace with answer
        }

        private string Replace(Match match, string current, FormAnswers formAnswers)
        {
            
            var answer = formAnswers.Pages.SelectMany(_ => _.Answers).FirstOrDefault(a => a.QuestionId.Equals(match.Value));

            return current.Replace(match.Groups[0].Value, answer.Response);
        }
    }

    public class ExternalDataEntity
    {
        public bool IsPost { get; set; }

        public string Url { get; set; }
    }
}