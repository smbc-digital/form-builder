﻿using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
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
            Properties.Hours = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, TimeConstants.HOURS_SUFFIX);
            Properties.Minutes = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, TimeConstants.MINUTES_SUFFIX);
            Properties.AmPm = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, TimeConstants.AM_PM_SUFFIX);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}