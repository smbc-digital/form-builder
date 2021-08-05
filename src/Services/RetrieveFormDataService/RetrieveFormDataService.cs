using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.RetrieveFormDataService
{
    public class RetrieveFormDataService : IRetrieveFormDataService
    {
        private readonly IGateway _gateway;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IElementMapper _elementMapper;
        private readonly IActionHelper _actionHelper;
        private readonly IWebHostEnvironment _environment;

        public RetrieveFormDataService(
            IGateway gateway,
            ISessionHelper sessionHelper,
            IDistributedCacheWrapper distributedCache,
            IActionHelper actionHelper,
            IWebHostEnvironment environment, 
            IElementMapper elementMapper)
        {
            _gateway = gateway;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
            _actionHelper = actionHelper;
            _environment = environment;
            _elementMapper = elementMapper;
        }

        public async Task Process(List<IAction> actions, FormSchema formSchema, string formName)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var formData = _distributedCache.GetString(sessionGuid);
            var formAnswers = new FormAnswers
            {
                FormName = formName,
                Pages = new List<PageAnswers>(),
                StartPageUrl = formSchema.StartPageUrl
            };

            if (!string.IsNullOrEmpty(formData))
                formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            if (formAnswers.Pages.Count == formSchema.Pages.Count(_ => _.ValidatableElements.Any()))
                return;

            foreach (var action in actions)
            {
                var response = new HttpResponseMessage();
                var submitSlug = action.Properties.PageActionSlugs.FirstOrDefault(_ =>
                    _.Environment.Equals(_environment.EnvironmentName.ToS3EnvPrefix(),
                        StringComparison.OrdinalIgnoreCase));

                if (submitSlug is null)
                    throw new ApplicationException(
                        "RetrieveFormDataService::Process, there is no PageActionSlug defined for this environment");

                var entity = _actionHelper.GenerateUrl(submitSlug.URL, formAnswers);

                if (!string.IsNullOrEmpty(submitSlug.AuthToken))
                    _gateway.ChangeAuthenticationHeader(submitSlug.AuthToken);

                response = await _gateway.GetAsync(entity.Url);

                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException(
                        $"RetrieveFormDataService::Process, http request to {entity.Url} returned an unsuccessful status code, Response: {JsonConvert.SerializeObject(response)}");

                if (response.Content is null)
                    throw new ApplicationException(
                        $"RetrieveFormDataService::Process, response content from {entity.Url} is null.");

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    throw new ApplicationException(
                        $"RetrieveFormDataService::Process, Gateway {entity.Url} responded with empty reference");

                var data = JsonConvert.DeserializeObject<ExpandoObject>(content) as IDictionary<string, dynamic>;

                foreach (var page in formSchema.Pages.Where(_ => _.ValidatableElements.Any()))
                {
                    var pageAnswers = new PageAnswers
                    {
                        Answers = new List<Answers>(), 
                        PageSlug = page.PageSlug
                    };

                    foreach (var element in page.ValidatableElements)
                    {
                        var responseAlreadyExists = formAnswers.AllAnswers.Any(_ => _.QuestionId.Equals(element.Properties.QuestionId));
                        if (!responseAlreadyExists)
                        {
                            pageAnswers.Answers.AddRange(SetAnswerValue(element, data));
                        }
                    }

                    formAnswers.Pages.Add(pageAnswers);
                }
            }

            await _distributedCache.SetStringAsync(sessionGuid, JsonConvert.SerializeObject(formAnswers));
        }

        private dynamic GetTargetMappedValue(string targetMapping, IDictionary<string, dynamic> obj)
        {
            var splitTargetMapping = targetMapping.Split(".");

            for (var i = 0; i < splitTargetMapping.Length; i++)
            {
                if (i == splitTargetMapping.Length - 1)
                {
                    return obj[splitTargetMapping[i]];
                }

                obj = (ExpandoObject)obj[splitTargetMapping[i]];
            }

            return string.Empty;
        }

        private List<Answers> SetAnswerValue(IElement element, IDictionary<string, dynamic> obj)
        {
            var key = element.Properties.QuestionId;
            var answerValue = string.IsNullOrEmpty(element.Properties.TargetMapping)
                ? obj[element.Properties.QuestionId]
                : GetTargetMappedValue(element.Properties.TargetMapping, obj);

            if (answerValue is null)
                return new List<Answers>();

            switch (element.Type)
            {
                case EElementType.DateInput:
                    return SetDateInputElementValue(key, answerValue);

                case EElementType.DatePicker:
                    return SetDatePickerElementValue(key, answerValue);

                case EElementType.Checkbox:
                    return SetCheckboxElementValue(key, answerValue as List<object>);

                default:
                    return new List<Answers>
                    {
                        new Answers
                        {
                            QuestionId = key,
                            Response = answerValue.ToString()
                        }
                    };
            }
        }

        private List<Answers> SetDateInputElementValue(string key, DateTime answer)
        {
            var answers = new List<Answers>();

            var dateDayKey = $"{key}-day";
            var dateMonthKey = $"{key}-month";
            var dateYearKey = $"{key}-year";

            answers.Add(new Answers { QuestionId = dateDayKey, Response = answer.Day.ToString() });
            answers.Add(new Answers { QuestionId = dateMonthKey, Response = answer.Month.ToString() });
            answers.Add(new Answers { QuestionId = dateYearKey, Response = answer.Year.ToString() });

            return answers;
        }

        private List<Answers> SetDatePickerElementValue(string key, DateTime answer)
        {
            return new List<Answers>
            {
                new Answers
                {
                    QuestionId = key,
                    Response = answer.Date.ToString("yyyy-MM-dd")
                }
            };
        }

        private List<Answers> SetCheckboxElementValue(string key, List<object> values)
        {
            var answer = string.Empty;

            foreach (var value in values)
            {
                if (string.IsNullOrEmpty(answer))
                    answer += (string) value;
                else
                    answer += $",{(string) value}";
            }

            return new List<Answers>
            {
                new Answers
                {
                    QuestionId = key,
                    Response = answer
                }
            };
        }
    }
}
