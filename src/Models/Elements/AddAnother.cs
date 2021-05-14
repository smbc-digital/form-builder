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
    public class AddAnother : Element
    {
        public AddAnother()
        {
            Type = EElementType.AddAnother;
        }

        public async override Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            return await Render(viewRender, elementHelper, guid, viewModel, page, formSchema, environment, formAnswers, results);
        }

        private async Task<string> Render(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            var subPath = viewModel["subPath"];

            var pageAnswers = new List<Answers>();
            if (formAnswers.Pages != null &&
                formAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(page.PageSlug)) != null)
            {
                pageAnswers = formAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(page.PageSlug)).Answers;
            }

            var listOfIncrementNumbers = new List<int>();
            foreach (var answer in pageAnswers)
            {
                int pFrom = answer.QuestionId.IndexOf("[") + "[".Length;
                int pTo = answer.QuestionId.LastIndexOf("]");

                listOfIncrementNumbers.Add(int.Parse(answer.QuestionId.Substring(pFrom, pTo - pFrom)));
            }

            var currentIncrement = listOfIncrementNumbers.Count > 0 ? listOfIncrementNumbers.Max(_ => _) : 0;

            // somehow get the increment value ie. how many fieldsets do we need to render
            bool addAnother = subPath.Equals("add-another");

            var increment = listOfIncrementNumbers.Count == 0 ? 0 : addAnother ? currentIncrement + 1 : currentIncrement;
            string html = "";
            for (var i = 0; i <= increment; i++)
            {
                html += $"<fieldset class='govuk-fieldset'><legend class='govuk-fieldset__legend govuk-fieldset__legend--m'>{Properties.Label}</legend>";
                if (increment > 0)
                {
                    html += $"<button data-prevent-double-click='true'data-disable-on-click = true class='govuk-button govuk-button--secondary' name='remove-{i}' id='remove-{i}' 'aria-describedby=remove' data-module='govuk-button'> Remove </button>";
                }
                foreach (var element in Properties.Elements)
                {
                    element.Properties.QuestionIdIncrement = i;
                    html += await element.RenderAsync(viewRender, elementHelper, guid, viewModel, page, formSchema,
                        environment, formAnswers, results);
                }

                html += "</fieldset>";
            }
            // add the addAnother button
            html += $"<button data-prevent-double-click='true'data-disable-on-click = true class='govuk-button govuk-button--secondary' name='addAnother' id='addAnother' 'aria-describedby=add-another' data-module='govuk-button'> Add another </button>";


            return html;
        }
    }
}
