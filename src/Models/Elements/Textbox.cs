using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace form_builder.Models.Elements
{
    public class Textbox : Element
    {
        public Textbox()
        {
            Type = EElementType.Textbox;
        }

        public override Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            var properties = new Dictionary<string, dynamic>()
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

            if(Properties.Telephone == true)
                properties["autocomplete"] = "tel";

            if (DisplayAriaDescribedby)
                properties.Add("aria-describedby", GetDescribedByAttributeValue());

            if (!string.IsNullOrEmpty(Properties.Purpose))
                properties.Add("autocomplete", Properties.Purpose);

            return properties;
        }
    }
}