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
        public virtual string QuestionId => Properties.QuestionId;
        public virtual string Label => Properties.Label;        
        public virtual string Hint => Properties.Hint;
        public virtual string HintId => $"{QuestionId}-hint"; 
        public virtual string ErrorId => $"{QuestionId}-error"; 
        public bool DisplayAriaDescribedby => DisplayHint || !IsValid; 
        public bool IsValid => validationResult.IsValid; 
        public string ValidationMessage => validationResult.Message;
        
        public virtual Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            return viewRender.RenderAsync(Type.ToString(), this, null);
        }
        
        public void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> validators)
        {
            foreach (var validator in validators)
            {
                var result = validator.Validate(this, viewModel);
                if (!result.IsValid)
                {
                    validationResult = result;
                    return;
                }
            }
        }

        public virtual string GenerateFieldsetProperties()
        {
            return string.Empty;
        }

        public virtual Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            return new Dictionary<string, dynamic>();
        }

        public Dictionary<string, dynamic> GenerateElementProperties(string errorMessage, string errorId)
        {
            if (Type == EElementType.AddressManual)
            {            
                var properties = new Dictionary<string, dynamic>();
                if(!IsValid && !string.IsNullOrEmpty(errorMessage))
                {
                    properties.Add("aria-describedby", errorId);
                }
                
                return properties;
            }

            return null;
        }

        public string GetListItemId(int index)
        {
            return $"{QuestionId}-{index}";
        }

        public string GetCustomItemId(string key)
        {
            return $"{QuestionId}-{key}";
        }

        public string GetCustomHintId(string key)
        {
            return $"{GetCustomItemId(key)}-hint";
        }

        public string GetCustomErrorId(string key)
        {
            return $"{GetCustomItemId(key)}-error";
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
            return CreateDescribedByAttributeValue($"{QuestionId}");
        }

        public string GetDescribedByAttributeValue(string prefix)
        {
            return CreateDescribedByAttributeValue($"{QuestionId}{prefix}");
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
                return $"{data} for = {QuestionId}{prefix}";
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

        public virtual string GetLabelText(){
            var optionalLabelText = Properties.Optional ? " (optional)" : string.Empty;

            return $"{Properties.Label}{optionalLabelText}";
        }
    }
}