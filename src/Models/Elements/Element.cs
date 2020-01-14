using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Properties;
using form_builder.Validators;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public BaseProperty Properties { get; set; }

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
                var fields = GetAllQuestionIds(this, viewModel);
                foreach (var field in fields)
                {
                    var result = validator.Validate(this, viewModel);

                    if (!result.IsValid)
                    {
                        validationResult = result;
                        return;
                    }
                }
            }
        }

        public Dictionary<string, object> GenerateElementProperties()
        {
            var properties = new Dictionary<string, object>();

            switch (Type)
            {
                case EElementType.Textbox:
                    properties = new Dictionary<string, object>()
                    {
                        { "name", Properties.QuestionId },
                        { "id", Properties.QuestionId },
                        { "maxlength", Properties.MaxLength },
                        { "value", Properties.Value},
                        { "autocomplete", "on" }
                    };

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue());
                    }

                    return properties;
                case EElementType.Textarea:
                    properties = new Dictionary<string, object>()
                    {
                        { "name", Properties.QuestionId },
                        { "id", Properties.QuestionId },
                        { "maxlength", Properties.MaxLength },
                        { "value", Properties.Value},
                        { "autocomplete", "on" }
                    };

                    if (Properties.MaxLength <= 200 || Properties.MaxLength > 500)
                    {
                        properties.Add("class", Properties.MaxLength > 500 ? "large" : "small");
                    }

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue());
                    }

                    return properties;
                case EElementType.Address:
                    properties = new Dictionary<string, object>()
                    {
                        { "id", $"{Properties.QuestionId}-postcode" },
                        { "maxlength", Properties.MaxLength }
                    };

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue("-postcode"));
                    }

                    return properties;
                case EElementType.AddressManual:
                    properties = new Dictionary<string, object>()
                    {                      
                    };
                    
                    return properties;
                case EElementType.Street:

                    properties = new Dictionary<string, object>()
                    {
                        { "id", $"{Properties.QuestionId}-street" },
                        { "maxlength", Properties.MaxLength }
                    };

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue("-street"));
                    }

                    return properties;
                case EElementType.Organisation:

                    properties = new Dictionary<string, object>()
                    {
                        { "id", $"{Properties.QuestionId}-organisation-searchterm" },
                        { "maxlength", Properties.MaxLength }
                    };

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue("-organisation-searchterm"));
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

        public Dictionary<string, object> GenerateElementProperties(string errorMessage, string errorId)
        {
            switch (Type)
            {
                case EElementType.AddressManual:
                    var properties = new Dictionary<string, object>();
                    if(!IsValid && !string.IsNullOrEmpty(errorMessage))
                    {
                        properties.Add("aria-describedby", errorId);
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

        public virtual Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
           
            
            return viewRender.RenderAsync(Type.ToString(), this, null);
        }

        public List<Element> GetAllQuestionIds(Element element, Dictionary<string, string> viewModel)
        {
            var list = viewModel.Where(x => x.Key.StartsWith(element.Properties.QuestionId)).ToList();
            var elementList = new List<Element>();

            foreach(var value in list)
            {
                var newElement = element;
                newElement.Properties.QuestionId = value.Key;
                newElement.Properties.Value = value.Value;


                elementList.Add(newElement);
            }

            return elementList;
        }

        private bool DisplayOptional
        {
            get
            {
                return Properties.Optional;
            }
        }
    }
}