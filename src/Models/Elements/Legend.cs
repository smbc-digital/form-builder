using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class Legend : Element
    {
        public Legend() => Type = EElementType.Legend;

        public override Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null) => viewRender.RenderAsync("LegendH1", this);
    }
}
