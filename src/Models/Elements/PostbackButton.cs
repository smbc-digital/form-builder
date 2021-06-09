using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class PostbackButton : Element
    {
        public PostbackButton()
        {
            Type = EElementType.PostbackButton;
        }

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
            return Task.FromResult($"<button formmethod='post' data-prevent-double-click='true'data-disable-on-click = true class='govuk-button govuk-button--secondary' name='{Properties.Name}' id='{Properties.Name}' 'aria-describedby={Properties.Name}' data-module='govuk-button'> {Properties.Label} </button>");
        } 
            
            
    }
}
