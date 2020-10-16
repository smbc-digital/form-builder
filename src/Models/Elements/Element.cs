using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators;
using Microsoft.AspNetCore.Hosting;

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

        public bool IsModelStateValid { get; set; } = true;

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

        public virtual string GenerateFieldsetProperties() => string.Empty;

        public virtual Dictionary<string, dynamic> GenerateElementProperties(string type = "") => new Dictionary<string, dynamic>();

        public string GetListItemId(int index) =>  $"{QuestionId}-{index}";

        public string GetCustomItemId(string key) => $"{QuestionId}-{key}";

        public string GetCustomHintId(string key) => $"{GetCustomItemId(key)}-hint";

        public string GetCustomErrorId(string key) =>  $"{GetCustomItemId(key)}-error";

        public string GetListItemHintId(int index) => $"{GetListItemId(index)}-hint";

        public string DescribedByAttribute() => DisplayAriaDescribedby ? $"aria-describedby=\"{GetDescribedByAttributeValue()}\"" : string.Empty;

        public string GetDescribedByAttributeValue() => CreateDescribedByAttributeValue($"{QuestionId}");

        public string GetDescribedByAttributeValue(string prefix) => CreateDescribedByAttributeValue($"{QuestionId}{prefix}");

        private string CreateDescribedByAttributeValue(string key)
        {
            var describedBy = new List<string>();
            if (DisplayHint)
                describedBy.Add(HintId);

            if (!IsValid)
                describedBy.Add(ErrorId);

            return string.Join(" ", describedBy);
        }

        public string WriteHtmlForAndClassAttribute(string prefix = "")
        {
            var data = string.Empty;

            if (DisplayOptional)
                data = "class = smbc-body";

            return !Properties.LegendAsH1 ? $"{data} for = {QuestionId}{prefix}" : data;
        }

        public string WriteOptional(string prefix = "") => DisplayOptional ? "class = optional" : null;

        public virtual Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null) => viewRender.RenderAsync(Type.ToString(), this, null);

        private bool DisplayOptional => Properties.Optional;

        public virtual string GetLabelText() => $"{Properties.Label}{(Properties.Optional ? " (optional)" : string.Empty)}";
    }
}