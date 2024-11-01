using form_builder.Constants;
using form_builder.Extensions;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.ValidateService
{
    public class ValidateService : IValidateService
    {
        private readonly IGateway _gateway;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IMappingService _mappingService;
        private readonly IActionHelper _actionHelper;
        private readonly IWebHostEnvironment _environment;

        public ValidateService(
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
                var response = new HttpResponseMessage();
                var submitSlug = action.Properties.PageActionSlugs.FirstOrDefault(_ =>
                    _.Environment.Equals(_environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase));

                if (submitSlug is null)
                    throw new ApplicationException("ValidateService::Process, there is no PageActionSlug defined for this environment");

                var entity = _actionHelper.GenerateUrl(submitSlug.URL, mappingData.FormAnswers);

                if (!string.IsNullOrEmpty(submitSlug.AuthToken))
                    _gateway.ChangeAuthenticationHeader(submitSlug.AuthToken);

                response = await _gateway.GetAsync(entity.Url);

                var responseAnswer = await response.Content.ReadAsStringAsync();
                answers.Add(new(ValidateConstants.ValidateId, responseAnswer));

                if (mappingData.FormAnswers.AdditionalFormData.TryGetValue(ValidateConstants.ValidateId, out object _))
                    mappingData.FormAnswers.AdditionalFormData.Remove(ValidateConstants.ValidateId);

                mappingData.FormAnswers.AdditionalFormData.Add(ValidateConstants.ValidateId, responseAnswer);

                await _distributedCache.SetStringAsync(formSessionId, JsonConvert.SerializeObject(mappingData.FormAnswers), CancellationToken.None);

                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"ValidateService::Process, http request to {entity.Url} returned an unsuccessful status code, Response: {JsonConvert.SerializeObject(response)}");
            }
        }
    }
}
