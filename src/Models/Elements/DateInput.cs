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
            Properties.Day = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, "-day");
            Properties.Month = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, "-month");
            Properties.Year = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, "-year");
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }

        public override void SetUpElementValue(IElementHelper elementHelper, Dictionary<string, dynamic> viewModel, FormAnswers formAnswers) {
            Properties.Day = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, "-day");
            Properties.Month = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, "-month");
            Properties.Year = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, "-year");
        }
    }
}