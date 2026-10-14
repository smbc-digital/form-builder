namespace form_builder.Services.RetrieveExternalDataService;

public class RetrieveExternalDataService(IGateway gateway,
    ISessionHelper sessionHelper,
    IDistributedCacheWrapper distributedCache,
    IMappingService mappingService,
    IActionHelper actionHelper,
    IWebHostEnvironment environment)
    : IRetrieveExternalDataService
{
    public async Task Process(List<IAction> actions, FormSchema formSchema, string formName)
    {
        List<Answers> answers = new();
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string formSessionId = $"{formName}::{browserSessionId}";
        var mappingData = await mappingService.Map(formSessionId, formName, null, formSchema);

        foreach (var action in actions)
        {
            var submitSlug = action.Properties.PageActionSlugs
                .FirstOrDefault(_ => _.Environment
                    .Equals(environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase));

            if (submitSlug is null)
                throw new ApplicationException("RetrieveExternalDataService::Process, there is no PageActionSlug defined for this environment");

            var entity = actionHelper.GenerateUrl(submitSlug.URL, mappingData.FormAnswers);

            if (!string.IsNullOrEmpty(submitSlug.AuthToken))
                gateway.ChangeAuthenticationHeader(submitSlug.AuthToken);

            var response = entity.IsPost ? await gateway.PostAsync(entity.Url, mappingData.Data) :
                await gateway.GetAsync(entity.Url);

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

        await distributedCache.SetStringAsync(formSessionId, JsonConvert.SerializeObject(mappingData.FormAnswers), CancellationToken.None);
    }
}