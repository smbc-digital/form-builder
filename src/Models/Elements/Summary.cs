using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;

namespace form_builder.Models.Elements
{
    public class Summary : Element
    {
        public Summary()
        {
            Type = EElementType.Summary;
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

            var htmlContent = new HtmlContentBuilder();
            var pages = elementHelper.GenerateQuestionAndAnswersList(guid, formSchema);

            htmlContent.AppendHtmlLine("<dl class=\"govuk-summary-list govuk-!-margin-bottom-9\">");
            foreach (var pageSummary in pages)
            {
                foreach (var answer in pageSummary.Answers)
                {
                    htmlContent.AppendHtmlLine("<div class=\"govuk-summary-list__row\">");
                    htmlContent.AppendHtmlLine($"<dt class=\"govuk-summary-list__key\">{answer.Key}</dt>");
                    htmlContent.AppendHtmlLine($"<dd class=\"govuk-summary-list__value\">{answer.Value}</dd>");

                    if (Properties != null && Properties.AllowEditing)
                        htmlContent.AppendHtmlLine($"<dd class=\"govuk-summary-list__actions\"><a class=\"govuk-link\" href=\"{pageSummary.PageSlug}\">Change</a><span class=\"govuk-visually-hidden\">{answer.Key}</span</dd>");

                    htmlContent.AppendHtmlLine("</div>");
                }
            }
            htmlContent.AppendHtmlLine("</dl>");

            using (var writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);

                return Task.FromResult(writer.ToString());
            }
        }
    }
}