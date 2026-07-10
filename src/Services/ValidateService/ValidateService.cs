namespace form_builder.Services.ValidateService;

public class ValidateService(IGateway gateway,
    ISessionHelper sessionHelper,
    IDistributedCacheWrapper distributedCache,
    IMappingService mappingService,
    IActionHelper actionHelper,
    IWebHostEnvironment environment)
    : IValidateService
{
    public async Task Process(List<IAction> actions, FormSchema formSchema, string formName)
    {
        List<Answers> answers = new();
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string formSessionId = $"{formName}::{browserSessionId}";
        var mappingData = await mappingService.Map(formSessionId, formName, null, formSchema);

        foreach (var action in actions)
        {
            var response = new HttpResponseMessage();
            var submitSlug = action.Properties.PageActionSlugs.FirstOrDefault(_ =>
                _.Environment.Equals(environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase));

            if (submitSlug is null)
                throw new ApplicationException("ValidateService::Process, there is no PageActionSlug defined for this environment");

            var entity = actionHelper.GenerateUrl(submitSlug.URL, mappingData.FormAnswers);

            if (!string.IsNullOrEmpty(submitSlug.AuthToken))
                gateway.ChangeAuthenticationHeader(submitSlug.AuthToken);

            response = await gateway.GetAsync(entity.Url);

            var responseAnswer = await response.Content.ReadAsStringAsync();
            answers.Add(new(ValidateConstants.ValidateId, responseAnswer));

            if (mappingData.FormAnswers.AdditionalFormData.TryGetValue(ValidateConstants.ValidateId, out object _))
                mappingData.FormAnswers.AdditionalFormData.Remove(ValidateConstants.ValidateId);

            mappingData.FormAnswers.AdditionalFormData.Add(ValidateConstants.ValidateId, responseAnswer);

            await distributedCache.SetStringAsync(formSessionId, JsonConvert.SerializeObject(mappingData.FormAnswers), CancellationToken.None);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"ValidateService::Process, http request to {entity.Url} returned an unsuccessful status code, Response: {JsonConvert.SerializeObject(response)}");
        }
    }
}