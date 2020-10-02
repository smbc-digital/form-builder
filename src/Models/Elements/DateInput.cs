using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace form_builder.Models.Elements
{
    public class DateInput : Element
    {
        public DateInput()
        {
            Type = EElementType.DateInput;
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
            Properties.Day = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-day");
            Properties.Month = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-month");
            Properties.Year = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-year");
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}