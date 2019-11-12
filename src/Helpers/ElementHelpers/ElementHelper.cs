using form_builder.Models;
using System;
using System.Collections.Generic;

namespace form_builder.Helpers.ElementHelpers
{
    public interface IElementHelper
    {
        string CurrentValue(Element element, Dictionary<string, string> viewModel);

        bool CheckForLabel(Element element, Dictionary<string, string> viewModel);

        bool CheckForMaxLength(Element element, Dictionary<string, string> viewModel);

        bool CheckIfLabelAndTextEmpty(Element element, Dictionary<string, string> viewModel);

        bool CheckForRadioOptions(Element element);
        bool CheckForSelectOptions(Element element);
        bool CheckForCheckBoxListValues(Element element);

    }

    public class ElementHelper : IElementHelper
    {
        public string CurrentValue(Element element, Dictionary<string, string> viewModel)
        {
            var currentValue = viewModel.ContainsKey(element.Properties.QuestionId);

            return currentValue ? viewModel[element.Properties.QuestionId] : string.Empty;
        }

        public bool CheckForLabel(Element element, Dictionary<string, string> viewModel)
        {
            if (string.IsNullOrEmpty(element.Properties.Label))
            {
                throw new Exception("No label found for element. Cannot render form.");
            }

            return true;
        }

        public bool CheckForMaxLength(Element element, Dictionary<string, string> viewModel)
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

        public bool CheckIfLabelAndTextEmpty(Element element, Dictionary<string, string> viewModel)
        {
            if(string.IsNullOrEmpty(element.Properties.Label) && string.IsNullOrEmpty(element.Properties.Text))
            {
                throw new Exception("An inline alert requires either a label or text or both to be present. Both can not be empty");
            }

            return true;
        }

        public bool CheckForRadioOptions(Element element)
        {
            if(element.Properties.Options == null || element.Properties.Options.Count <= 1)
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
    }
}
