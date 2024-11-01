using System.Net;
using form_builder.Extensions;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.RetrieveExternalDataService
{
    public class RetrieveExternalDataService : IRetrieveExternalDataService
    {
        private readonly IGateway _gateway;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IMappingService _mappingService;
        private readonly IActionHelper _actionHelper;
        private readonly IWebHostEnvironment _environment;

        public RetrieveExternalDataService(
            IGateway gateway,
            ISessionHelper sessionHelper,
            IDistributedCacheWrapper distributedCache,
            IMappingService mappingService,
            IActionHelper actionHelper,
            IWebHostEnvironment environment)
        {
            _gateway = gateway;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
            _mappingService = mappingService;
            _actionHelper = actionHelper;
            _environment = environment;
        }

        public async Task Process(List<IAction> actions, FormSchema formSchema, string formName)
        {
            List<Answers> answers = new();
            ISession browserSessionId = _sessionHelper.GetSession();
            string formSessionId = $"{formName}::{browserSessionId.Id}";
            var mappingData = await _mappingService.Map(formSessionId, formName);

            foreach (var action in actions)
            {
                var submitSlug = action.Properties.PageActionSlugs
                    .FirstOrDefault(_ => _.Environment
                        .Equals(_environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase));

                if (submitSlug is null)
                    throw new ApplicationException("RetrieveExternalDataService::Process, there is no PageActionSlug defined for this environment");

                var entity = _actionHelper.GenerateUrl(submitSlug.URL, mappingData.FormAnswers);

                if (!string.IsNullOrEmpty(submitSlug.AuthToken))
                    _gateway.ChangeAuthenticationHeader(submitSlug.AuthToken);

                var response = entity.IsPost ? await _gateway.PostAsync(entity.Url, mappingData.Data) :
                        await _gateway.GetAsync(entity.Url);

                var responseAnswer = string.Empty;
                if (response.StatusCode.Equals(HttpStatusCode.NotFound) || response.StatusCode.Equals(HttpStatusCode.NoContent))
                {
                    responseAnswer = null;
                }
                else if (response.IsSuccessStatusCode)
                {
                    if (response.Content is null)
                        throw new ApplicationException($"RetrieveExternalDataService::Process, http request to {entity.Url} returned an null content, Response: {JsonConvert.SerializeObject(response)}");

                    string content = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(content))
                        throw new ApplicationException($"RetrieveExternalDataService::Process, http request to {entity.Url} returned an null or empty content, Response: {JsonConvert.SerializeObject(response)}");

                    responseAnswer = System.Text.Json.JsonSerializer.Deserialize<string>(content);
                }
                else
                {
                    throw new ApplicationException($"RetrieveExternalDataService::Process, http request to {entity.Url} returned an unsuccessful status code, Response: {JsonConvert.SerializeObject(response)}");
                }

                Answers answer = new(action.Properties.TargetQuestionId, responseAnswer);
                answers.Add(answer);

                if (action.Properties.IncludeInFormSubmission)
                {
                    if (mappingData.FormAnswers.AdditionalFormData.TryGetValue(answer.QuestionId, out object _))
                        mappingData.FormAnswers.AdditionalFormData.Remove(answer.QuestionId);

                    mappingData.FormAnswers.AdditionalFormData.Add(answer.QuestionId, answer.Response);
                }
            }

            if (mappingData.FormAnswers.Pages.Any())
            {
                mappingData.FormAnswers.Pages
                    .First(_ => _.PageSlug.Equals(mappingData.FormAnswers.Path, StringComparison.OrdinalIgnoreCase))
                    .Answers.AddRange(answers);
            }

            await _distributedCache.SetStringAsync(formSessionId, JsonConvert.SerializeObject(mappingData.FormAnswers), CancellationToken.None);
        }
    }
}