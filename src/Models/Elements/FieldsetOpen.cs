using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class FieldsetOpen : Element
    {
        public FieldsetOpen()
        {
            Type = EElementType.FieldsetOpen;
        }

        public override Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null) => Task.FromResult($"<fieldset class='govuk-fieldset'><legend class='govuk-fieldset__legend govuk-fieldset__legend--m'>{Properties.Label}</legend>");
    }
}
