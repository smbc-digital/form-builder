using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Validators;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class Element : IElement
    {
        private ValidationResult validationResult;

        public Element()
        {
            validationResult = new ValidationResult();
        }

        public EElementType Type { get; set; }

        public Property Properties { get; set; }

        public bool DisplayAriaDescribedby
        {
            get
            {
                return Properties.Hint != string.Empty || !IsValid;
            }
        }

        public bool IsValid
        {
            get
            {
                return validationResult.IsValid;
            }
        }

        public string ValidationMessage
        {
            get
            {
                return validationResult.Message;
            }
        }

        public void Validate(Dictionary<string, string> viewModel, IEnumerable<IElementValidator> form_builder)
        {
            foreach (var validator in form_builder)
            {
                var result = validator.Validate(this, viewModel);
                if (!result.IsValid)
                {
                    validationResult = result;
                    return;
                }
            }
        }

        public Dictionary<string, object> GenerateElementProperties()
        {
            var maxLength = string.IsNullOrEmpty(Properties.MaxLength) ? "200" : Properties.MaxLength;
            var properties = new Dictionary<string, object>();

            switch (Type)
            {
                case EElementType.Textbox:
                case EElementType.Textarea:
                    properties = new Dictionary<string, object>()
                    {
                        { "name", Properties.QuestionId },
                        { "id", Properties.QuestionId },
                        { "maxlength", maxLength },
                        { "value", Properties.Value},
                        { "autocomplete", "on" }
                    };

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue());
                    }

                    return properties;
                case EElementType.Address:
                    properties = new Dictionary<string, object>()
                    {
                        { "id", $"{Properties.QuestionId}-postcode" },
                        { "maxlength", maxLength }
                    };

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue("-postcode"));
                    }

                    return properties;
                case EElementType.Street:

                    properties = new Dictionary<string, object>()
                    {
                        { "id", $"{Properties.QuestionId}-street" },
                        { "maxlength", maxLength }
                    };

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue("-street"));
                    }

                    return properties;
                default:
                    return null;
            }
        }

        public Dictionary<string, object> GenerateElementProperties(int index)
        {
            switch (Type)
            {
                case EElementType.Radio:
                    var properties = new Dictionary<string, object>()
                    {
                        {"name", Properties.QuestionId },
                        { "id", $"{Properties.QuestionId}-{index}" },
                        { "value", Properties.Value}
                    };

                    if (!string.IsNullOrEmpty(Properties.Options[index].Hint))
                    {
                        properties.Add("aria-describedby", $"{Properties.QuestionId}-{index}-hint");
                    }

                    return properties;
                default:
                    return null;
            }
        }

        public string DescribedByValue()
        {
            return DescribeValue($"{Properties.QuestionId}");
        }

        public string DescribedByValue(string prefix)
        {
            return DescribeValue($"{Properties.QuestionId}{prefix}");
        }

        private string DescribeValue(string key)
        {
            var describedByValue = string.Empty;

            if (!string.IsNullOrEmpty(Properties.Hint))
            {
                describedByValue += $"{key}-hint ";
            }

            if (!IsValid)
            {
                describedByValue += $"{key}-error";
            }

            return describedByValue.Trim();
        }

        public string WriteOptional()
        {
            if (DisplayOptional)
            {
                return "class = optional";
            }

            return null;
        }

        public Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            return viewRender.RenderAsync(Type.ToString(), this, null);
        }

        private bool DisplayOptional
        {
            get
            {
                return Properties?.Optional ?? true;
            }
        }
    }
}