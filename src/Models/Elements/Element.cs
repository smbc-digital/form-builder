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

        public string WriteOptional(string prefix = "")
        {
            if (DisplayOptional)
            {
                return "class = optional";

            }

            return null;
        }

        public virtual Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            return viewRender.RenderAsync(Type.ToString(), this, null);
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