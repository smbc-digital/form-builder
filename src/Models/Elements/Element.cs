using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Properties;
using form_builder.Validators;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
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
        public BaseProperty Properties { get; set; }
        public bool DisplayHint => !string.IsNullOrEmpty(Properties.Hint.Trim());
        public bool HadCustomClasses => !string.IsNullOrEmpty(Properties.ClassName);
        public string HintId => $"{Properties.QuestionId}-hint"; 
        public string ErrorId => $"{Properties.QuestionId}-error"; 
        public bool DisplayAriaDescribedby => DisplayHint || !IsValid; 
        public bool IsValid => validationResult.IsValid; 
        public string ValidationMessage => string.IsNullOrEmpty(Properties.CustomValidationMessage) ? validationResult.Message : Properties.CustomValidationMessage; 
        
        public virtual Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            return viewRender.RenderAsync(Type.ToString(), this, null);
        }
        
        public void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> form_builder)
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

        public virtual Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            return new Dictionary<string, dynamic>();
        }

        // TODO: What is the point in this?
        public virtual string GenerateFieldsetProperties()
        {
            return string.Empty;
        }

        public Dictionary<string, dynamic> GenerateElementProperties(int index)
        {
            switch (Type)
            {
                case EElementType.Radio:
                    var properties = new Dictionary<string, dynamic>()
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

        public Dictionary<string, dynamic> GenerateElementProperties(string errorMessage, string errorId)
        {
            switch (Type)
            {
                case EElementType.AddressManual:
                    var properties = new Dictionary<string, dynamic>();
                    if(!IsValid && !string.IsNullOrEmpty(errorMessage))
                    {
                        properties.Add("aria-describedby", errorId);
                    }
                    return properties;
                default:
                    return null;
            }
            
        }

        public string GetListItemId(int index)
        {
            return $"{Properties.QuestionId}-{index}";
        }
        public string GetListItemHintId(int index)
        {
            return $"{GetListItemId(index)}-hint";
        }

        public string DescribedByAttribute()
        {
            return DisplayAriaDescribedby ? $"aria-describedby=\"{GetDescribedByAttributeValue()}\"" : string.Empty;
        }

        public string GetDescribedByAttributeValue()
        {
            return CreateDescribedByAttributeValue($"{Properties.QuestionId}");
        }

        public string GetDescribedByAttributeValue(string prefix)
        {
            return CreateDescribedByAttributeValue($"{Properties.QuestionId}{prefix}");
        }

        private string CreateDescribedByAttributeValue(string key)
        {
            var describedBy = new List<string>();
            if (DisplayHint)
            {
                describedBy.Add(HintId);
            }

            if (!IsValid)
            {
                describedBy.Add(ErrorId);
            }

            return string.Join(" ", describedBy);
        }

        public string WriteHtmlForAndClassAttribute(string prefix = "")
        {
            var data = string.Empty;

            if (DisplayOptional)
            {
                data = "class = optional";
            }

            if (!Properties.LegendAsH1)
            {
                return $"{data} for = {Properties.QuestionId}{prefix}";
            }

            return data;
        }

        // TODO: We should be able to get rid of this eventually
        public string WriteOptional(string prefix = "")
        {
            if (DisplayOptional)
            {
                return "class = optional";
            }

            return null;
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