using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class DateInput : Element
    {
        public DateInput()
        {
            Type = EElementType.DateInput;
        }

        public static string DAY_EXTENSION => "-day";
        public static string MONTH_EXTENSION => "-month";
        public static string YEAR_EXTENSION => "-year";

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
            Properties.Day = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, DAY_EXTENSION);
            Properties.Month = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, MONTH_EXTENSION);
            Properties.Year = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, YEAR_EXTENSION);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }

        // TODO: This needs tests
        private static bool HasValidDateTimeAnswer(Dictionary<string, dynamic> viewModel, string key)
        {
                if (viewModel.ContainsKey($"{key}{DateInput.YEAR_EXTENSION}") && 
                viewModel.ContainsKey($"{key}{DateInput.MONTH_EXTENSION}") && 
                viewModel.ContainsKey($"{key}{DateInput.DAY_EXTENSION}"))
                {
                    return true;
                }
                
                return false;
        }

        // TODO: This needs tests
        public static DateTime? GetDate(Dictionary<string, dynamic> viewModel, string key)
        {
            if(!HasValidDateTimeAnswer(viewModel, key))            
                return null;

            var year = viewModel[$"{key}{DateInput.YEAR_EXTENSION}"];
            var month = viewModel[$"{key}{DateInput.MONTH_EXTENSION}"];
            var day = viewModel[$"{key}{DateInput.DAY_EXTENSION}"];
            
            return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
        }

        // TODO: This needs tests
        public static DateTime? GetDate(FormAnswers answers, string key)
        {
            var flattenedAnswers = answers.AllAnswers;
            var year = flattenedAnswers.Single(answer => answer.QuestionId == $"{key}{DateInput.YEAR_EXTENSION}").Response; 
            var month = flattenedAnswers.Single(answer => answer.QuestionId == $"{key}{DateInput.MONTH_EXTENSION}").Response;
            var day = flattenedAnswers.Single(answer => answer.QuestionId == $"{key}{DateInput.DAY_EXTENSION}").Response;
            
            return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
        }
    }
}