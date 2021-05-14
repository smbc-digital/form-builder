using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class TimeInput : Element
    {
        public TimeInput()
        {
            Type = EElementType.TimeInput;
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
            Properties.Hours = Properties.IsAddAnotherElement ?
                elementHelper.CurrentValue($"{Properties.QuestionId}{TimeConstants.HOURS_SUFFIX}_{Properties.QuestionIdIncrement}", viewModel, formAnswers, "") :
                elementHelper.CurrentValue($"{Properties.QuestionId}", viewModel, formAnswers, TimeConstants.HOURS_SUFFIX);

            Properties.Minutes = Properties.IsAddAnotherElement ?
                elementHelper.CurrentValue($"{Properties.QuestionId}{TimeConstants.MINUTES_SUFFIX}_{Properties.QuestionIdIncrement}", viewModel, formAnswers, "") :
                elementHelper.CurrentValue($"{Properties.QuestionId}", viewModel, formAnswers, TimeConstants.MINUTES_SUFFIX);

            Properties.AmPm = Properties.IsAddAnotherElement ?
                elementHelper.CurrentValue($"{Properties.QuestionId}{TimeConstants.AM_PM_SUFFIX}_{Properties.QuestionIdIncrement}", viewModel, formAnswers, "") :
                elementHelper.CurrentValue($"{Properties.QuestionId}", viewModel, formAnswers, TimeConstants.AM_PM_SUFFIX);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}