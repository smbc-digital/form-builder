using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class Textbox : Element
    {
        public bool HasPrefix => !string.IsNullOrEmpty(Properties.Prefix);
        public Textbox() => Type = EElementType.Textbox;

        public override Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            var properties = new Dictionary<string, dynamic>
            {
                { "name", Properties.QuestionId },
                { "id", Properties.QuestionId },
                { "maxlength", Properties.MaxLength },
                { "value", Properties.Value},
                { "spellcheck", Properties.Spellcheck.ToString().ToLower() }
            };

            if (Properties.Numeric)
            {
                properties.Add("type", "number");
                properties.Add("max", Properties.Max);
                properties.Add("min", Properties.Min);
            }

            if (DisplayAriaDescribedby)
                properties.Add("aria-describedby", GetDescribedByAttributeValue());

            if (!string.IsNullOrEmpty(Properties.Purpose))
                properties.Add("autocomplete", Properties.Purpose);

            return properties;
        }
    }
}