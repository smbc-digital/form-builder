using form_builder.Models;
using form_builder.Providers.StorageProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Helpers.ElementHelpers
{
    public interface IElementHelper
    {
        string CurrentValue(Element element, Dictionary<string, string> viewModel, string pageSlug, string guid, string suffix = "");
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
    }

    public class ElementHelper : IElementHelper
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        public ElementHelper(IDistributedCacheWrapper distributedCacheWrapper)
        {
            _distributedCache = distributedCacheWrapper;
        }

        public string CurrentValue(Element element, Dictionary<string, string> viewModel, string pageSlug, string guid, string suffix = "")
        {
            var currentValue = viewModel.ContainsKey($"{element.Properties.QuestionId}{suffix}");
            var cacheData = _distributedCache.GetString(guid);

            if (!currentValue && cacheData != null)
            {
                var mappedCacheData = Newtonsoft.Json.JsonConvert.DeserializeObject<FormAnswers>(cacheData);
                var storedValue = mappedCacheData.Pages.FirstOrDefault(_ => _.PageSlug == pageSlug);

                if (storedValue != null)
                {
                    var value = storedValue.Answers.FirstOrDefault(_ => _.QuestionId == $"{element.Properties.QuestionId}{suffix}");

                    return value != null ? value.Response : string.Empty;
                }

                return string.Empty;
            }

            return currentValue ? viewModel[$"{element.Properties.QuestionId}{suffix}"] : string.Empty;
        }

        public bool CheckForLabel(Element element)
        {
            if (string.IsNullOrEmpty(element.Properties.Label))
            {
                throw new Exception("No label found for element. Cannot render form.");
            }

            return true;
        }

        public bool CheckForQuestionId(Element element)
        {
            if (string.IsNullOrEmpty(element.Properties.QuestionId))
            {
                throw new Exception("No question id found for element. Cannot render form.");
            }

            return true;
        }

        public bool CheckForMaxLength(Element element)
        {
            if (string.IsNullOrEmpty(element.Properties.MaxLength))
            {
                throw new Exception("No Max Length found for element. Cannot render form.");
            }

            if (Int32.Parse(element.Properties.MaxLength) < 1)
            {
                throw new Exception("Max Length must be greater than zero. Cannot render form.");
            }

            return true;
        }

        public bool CheckIfLabelAndTextEmpty(Element element)
        {
            if (string.IsNullOrEmpty(element.Properties.Label) && string.IsNullOrEmpty(element.Properties.Text))
            {
                throw new Exception("An inline alert requires either a label or text or both to be present. Both can not be empty");
            }

            return true;
        }

        public bool CheckForRadioOptions(Element element)
        {
            if (element.Properties.Options == null || element.Properties.Options.Count <= 1)
            {
                throw new Exception("A radio element requires two or more options to be present.");
            }
            return true;
        }

        public bool CheckForSelectOptions(Element element)
        {
            if (element.Properties.Options == null || element.Properties.Options.Count <= 1)
            {
                throw new Exception("A select element requires two or more options to be present.");
            }

            return true;
        }

        public bool CheckForCheckBoxListValues(Element element)
        {
            if (element.Properties.Options == null || element.Properties.Options.Count < 1)
            {
                throw new Exception("A checkbox list requires one or more options to be present.");
            }
            return true;
        }

        public bool CheckAllDateRestrictionsAreNotEnabled(Element element)
        {
            if (element.Properties.RestrictCurrentDate && element.Properties.RestrictPastDate && element.Properties.RestrictFutureDate)
            {
                throw new Exception("Cannot set all date restrictions to true");
            }
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
    }
}