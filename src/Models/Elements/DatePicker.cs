using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class DatePicker : Element
    {

        public DatePicker()
        {
            Type = EElementType.DatePicker;
        }

        public override Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IHostingEnvironment environment,
            List<object> results = null)
        {
            Properties.Date = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, string.Empty);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type)
        {
            var todaysDate = DateTime.Now;
            var maxDate = Properties.RestrictFutureDate ?
                (Properties.RestrictCurrentDate ? DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd"))
                : string.IsNullOrEmpty(Properties.Max) ? todaysDate.AddYears(100).ToString("yyyy-MM-dd") : new DateTime(int.Parse(Properties.Max), todaysDate.Month, todaysDate.Day).ToString("yyyy-MM-dd");

            var minDate = Properties.RestrictPastDate ?
                (Properties.RestrictCurrentDate ? DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd"))
                : string.Empty;

            var properties = new Dictionary<string, dynamic>()
            {
                { "type", "date" },
                { "id", Properties.QuestionId },
                { "name", Properties.QuestionId },
                { "max", maxDate },
                { "min", minDate }
            };

            return properties;
        }
    }
}
