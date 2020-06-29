using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Properties;
using form_builder.Validators;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class Element : IElement
    {
        protected ValidationResult validationResult;

        public Element()
        {
            validationResult = new ValidationResult();
        }

        public EElementType Type { get; set; }
        public BaseProperty Properties { get; set; }
        public virtual bool DisplayHint => !string.IsNullOrEmpty(Properties.Hint.Trim());
        public bool HadCustomClasses => !string.IsNullOrEmpty(Properties.ClassName);
        public virtual string QuestionId => Properties.QuestionId;
        public virtual string Label => Properties.Label;        
        public virtual string Hint => Properties.Hint;
        public virtual string HintId => $"{QuestionId}-hint"; 
        public virtual string ErrorId => $"{QuestionId}-error"; 
        public bool DisplayAriaDescribedby => DisplayHint || !IsValid; 
        public bool IsValid => validationResult.IsValid; 
        public string ValidationMessage => validationResult.Message;
        public string Lookup { get; set; }
        
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
                data = "class = smbc-body";
            }

            if (!Properties.LegendAsH1)
            {
                return $"{data} for = {QuestionId}{prefix}";
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

        public virtual Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IHostingEnvironment environment,
            List<object> results = null)
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

        public virtual string GetLabelText() => $"{Properties.Label}{(Properties.Optional ? " (optional)" : string.Empty)}";
    }
}