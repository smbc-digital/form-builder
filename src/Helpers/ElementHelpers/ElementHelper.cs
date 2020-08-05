using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;

namespace form_builder.Helpers.ElementHelpers
{
    public interface IElementHelper
    {
        T CurrentValue<T>(Element element, Dictionary<string, dynamic> viewModel, string pageSlug, string guid, string suffix = "");
        
        bool CheckForQuestionId(Element element);
        
        bool CheckForLabel(Element element);
        
        bool CheckForMaxLength(Element element);
        
        bool CheckIfLabelAndTextEmpty(Element element);
        
        bool CheckForRadioOptions(Element element);
        
        bool CheckForSelectOptions(Element element);
        
        bool CheckForCheckBoxListValues(Element element);
        
        bool CheckAllDateRestrictionsAreNotEnabled(Element element);
        
        void ReSelectPreviousSelectedOptions(Element element);
        
        void ReCheckPreviousRadioOptions(Element element);
        
        bool CheckForProvider(Element element);
        
        object GetFormDataValue(string guid, string key);
        
        FormAnswers GetFormData(string guid);
        
        List <PageSummary> GenerateQuestionAndAnswersList(string guid, FormSchema formSchema);
    }

    public class ElementHelper : IElementHelper
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IElementMapper _elementMapper;
        public ElementHelper(IDistributedCacheWrapper distributedCacheWrapper, IElementMapper elementMapper)
        {
            _distributedCache = distributedCacheWrapper;
            _elementMapper = elementMapper;
        }

        public T CurrentValue<T>(Element element, Dictionary<string, dynamic> answers, string pageSlug, string guid, string suffix = "")
        {
            var defaultValue = (T) Convert.ChangeType(string.Empty, typeof(T));

            if (element.Type == EElementType.FileUpload)
                return defaultValue;

            var currentValue = answers.ContainsKey($"{element.Properties.QuestionId}{suffix}");

            if (!currentValue)
            {
                var cacheData = _distributedCache.GetString(guid);  
                if(cacheData != null)
                {
                    var mappedCacheData = JsonConvert.DeserializeObject<FormAnswers>(cacheData);
                    var storedValue = mappedCacheData.Pages.FirstOrDefault(_ => _.PageSlug == pageSlug);

                    if (storedValue != null)
                    {
                        var value = storedValue.Answers.FirstOrDefault(_ => _.QuestionId == $"{element.Properties.QuestionId}{suffix}");

                        return value != null ? (T)value.Response : defaultValue;
                    }
                }
                return defaultValue;
            }

            return answers[$"{element.Properties.QuestionId}{suffix}"];
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
                throw new Exception("An inline alert requires either a label or text or both to be present. Both can not be empty");

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
            var pages = formSchema.Pages.ToList();

            foreach (var page in pages)
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

                if(formSchemaQuestions.Any() || !reducedAnswers.Where(p => p.PageSlug == page.PageSlug).Select(p => p).Any())                    
                    continue;

                formSchemaQuestions.ForEach(question => {
                    var answer = _elementMapper.GetAnswerStringValue(question, formAnswers);
                    summaryBuilder.Add(question.GetLabelText(), answer, question.Type);
                });

                pageSummary.Answers = summaryBuilder.Build();
                formSummary.Add(pageSummary);
            }

            return formSummary;
        }
    }
}
