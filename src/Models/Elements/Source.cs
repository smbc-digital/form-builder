using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class Source : Element
    {
        public Source()
        {
            Type = EElementType.Source;
        }

        public override async Task<string> RenderAsync(IViewRender viewRender,
           IElementHelper elementHelper,
           string guid,
           Dictionary<string, dynamic> viewModel,
           Page page,
           FormSchema formSchema,
           IWebHostEnvironment environment,
           FormAnswers formAnswers,
           List<object> results = null)
        {
            return null;
        }
    }
}
