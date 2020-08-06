using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class DateInput : Element
    {
        public DateInput()
        {
            Type = EElementType.DateInput;
        }

        public override Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            List<object> results = null)
        {
            Properties.Day = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, "-day");
            Properties.Month = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, "-month");
            Properties.Year = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, "-year");
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}