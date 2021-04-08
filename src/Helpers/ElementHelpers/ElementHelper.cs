using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace form_builder.Helpers.ElementHelpers
{
    public class ElementHelper : IElementHelper
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IElementMapper _elementMapper;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ElementHelper(IDistributedCacheWrapper distributedCacheWrapper,
            IElementMapper elementMapper,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _distributedCache = distributedCacheWrapper;
            _elementMapper = elementMapper;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public string CurrentValue(string questionId, Dictionary<string, dynamic> viewmodel, FormAnswers answers, string suffix = "")
        {
            //Todo
            var defaultValue = string.Empty;

            return GetCurrentValueFromSavedAnswers<string>(defaultValue, questionId, viewmodel, answers, suffix);
        }

        public T CurrentValue<T>(string questionId, Dictionary<string, dynamic> viewmodel, FormAnswers answers, string suffix = "") where T : new()
        {
            //Todo
            var defaultValue = new T();

            return GetCurrentValueFromSavedAnswers<T>(defaultValue, questionId, viewmodel, answers, suffix);
        }

        private T GetCurrentValueFromSavedAnswers<T>(T defaultValue, string questionId, Dictionary<string, dynamic> viewmodel, FormAnswers answers, string suffix)
        {
            var currentValue = viewmodel.ContainsKey($"{questionId}{suffix}");

            if (!currentValue)
            {
                var storedValue = answers.Pages?.SelectMany(_ => _.Answers);

                if (storedValue != null)
                {
                    var value = storedValue.FirstOrDefault(_ => _.QuestionId == $"{questionId}{suffix}");
                    return value != null ? (T)value.Response : defaultValue;
                }
                return defaultValue;
            }

            return viewmodel[$"{questionId}{suffix}"];
        }

        public bool CheckForLabel(Element element)
        {
            if (string.IsNullOrEmpty(element.Properties.Label))
                throw new Exception("No label found for element. Cannot render form.");

            return true;
        }

        public bool CheckForQuestionId(Element element)
        {
            if (string.IsNullOrEmpty(element.Properties.QuestionId))
                throw new Exception("No question id found for element. Cannot render form.");

            return true;
        }

        public bool CheckForMaxLength(Element element)
        {
            if (element.Properties.MaxLength < 1)
                throw new Exception("Max Length must be greater than zero. Cannot render form.");

            return true;
        }

        public bool CheckIfLabelAndTextEmpty(Element element)
        {
            if (string.IsNullOrEmpty(element.Properties.Label) && string.IsNullOrEmpty(element.Properties.Text))
                throw new Exception("An inline alert requires either a label or text or both to be present. Both cannot be empty");

            return true;
        }

        public bool CheckForRadioOptions(Element element)
        {
            if (element.Properties.Options == null || element.Properties.Options.Count <= 1)
                throw new Exception("A radio element requires two or more options to be present.");

            return true;
        }

        public bool CheckForSelectOptions(Element element)
        {
            if (element.Properties.Options == null || element.Properties.Options.Count <= 1)
                throw new Exception("A select element requires two or more options to be present.");

            return true;
        }

        public bool CheckForCheckBoxListValues(Element element)
        {
            if (element.Properties.Options == null || element.Properties.Options.Count < 1)
                throw new Exception("A checkbox list requires one or more options to be present.");

            return true;
        }

        public bool CheckAllDateRestrictionsAreNotEnabled(Element element)
        {
            if (element.Properties.RestrictCurrentDate && element.Properties.RestrictPastDate && element.Properties.RestrictFutureDate)
                throw new Exception("Cannot set all date restrictions to true");

            return true;
        }

        public void ReSelectPreviousSelectedOptions(Element element)
        {
            foreach (var option in element.Properties.Options)
            {
                if (option.Value == element.Properties.Value)
                {
                    option.Selected = true;
                }
                else
                {
                    option.Selected = false;
                }
            }
        }

        public void ReCheckPreviousRadioOptions(Element element)
        {
            foreach (var option in element.Properties.Options)
            {
                if (option.Value == element.Properties.Value)
                {
                    option.Checked = true;
                }
                else
                {
                    option.Checked = false;
                }
            }
        }

        public bool CheckForProvider(Element element)
        {
            if (string.IsNullOrEmpty(element.Properties.StreetProvider) && element.Type == EElementType.Street
                  || string.IsNullOrEmpty(element.Properties.AddressProvider) && element.Type == EElementType.Address
                  || string.IsNullOrEmpty(element.Properties.OrganisationProvider) && element.Type == EElementType.Organisation)
                throw new Exception($"A {element.Type} Provider must be present.");

            return true;
        }

        public object GetFormDataValue(string guid, string key)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            return convertedAnswers.FormData.ContainsKey(key) ? convertedAnswers.FormData.GetValueOrDefault(key) : string.Empty;
        }

        public FormAnswers GetFormData(string guid)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            return convertedAnswers;
        }

        public List<PageSummary> GenerateQuestionAndAnswersList(string guid, FormSchema formSchema)
        {
            var formAnswers = GetFormData(guid);
            var reducedAnswers = FormAnswersExtensions.GetReducedAnswers(formAnswers, formSchema);
            var formSummary = new List<PageSummary>();

            foreach (var page in formSchema.Pages.ToList())
            {
                var pageSummary = new PageSummary
                {
                    PageTitle = page.Title,
                    PageSlug = page.PageSlug
                };

                var summaryBuilder = new SummaryDictionaryBuilder();
                var formSchemaQuestions = page.ValidatableElements
                    .Where(_ => _ != null)
                    .ToList();

                if (!formSchemaQuestions.Any() || !reducedAnswers.Where(p => p.PageSlug == page.PageSlug).Select(p => p).Any())
                    continue;

                formSchemaQuestions.ForEach(question =>
                {
                    var answer = _elementMapper.GetAnswerStringValue(question, formAnswers);
                    summaryBuilder.Add(question.GetLabelText(page.Title), answer, question.Type);
                });

                pageSummary.Answers = summaryBuilder.Build();
                formSummary.Add(pageSummary);
            }

            return formSummary;
        }

        public string GenerateDocumentUploadUrl(Element element, FormSchema formSchema, FormAnswers formAnswers)
        {
            var urlOrigin = $"https://{_httpContextAccessor.HttpContext.Request.Host}/";
            var urlPath = $"{formSchema.BaseURL}/{FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH}{SystemConstants.CaseReferenceQueryString}{Convert.ToBase64String(Encoding.ASCII.GetBytes(formAnswers.CaseReference))}";

            return _environment.EnvironmentName.Equals("local")
                ? $"{urlOrigin}{urlPath}"
                : $"{urlOrigin}v2/{urlPath}";
        }

        public void OrderOptionsAlphabetically(Element element)
        {
            if (element.Properties.OrderOptionsAlphabetically)
                element.Properties.Options.Sort((x, y) => x.Text.CompareTo(y.Text));
        }
    }
}
