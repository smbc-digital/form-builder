using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;


namespace form_builder.Models.Elements
{
    public class Summary : Element
    {
        public Summary()
        {
            Type = EElementType.Summary;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {

            var htmlContent = new HtmlContentBuilder();
            var pages = elementHelper.GenerateQuestionAndAnswersList(guid, formSchema);

            foreach (var pageSummary in pages)
            { 
                htmlContent.AppendHtmlLine("<dl class=\"govuk-summary-list govuk-!-margin-bottom-9\">");
               foreach (var answer in pageSummary.Answers)
                {
                    htmlContent.AppendHtmlLine("<div class=\"govuk-summary-list__row\">");
                    htmlContent.AppendHtmlLine($"<dt class=\"govuk-summary-list__key\">{answer.Key}</dt>");
                    htmlContent.AppendHtmlLine($"<dd class=\"govuk-summary-list__value\">{answer.Value}</dd>");

                    if (Properties != null && Properties.AllowEditing)
                    {
                        htmlContent.AppendHtmlLine($"<dd class=\"govuk-summary-list__actions\"><a class=\"govuk-link\" href=\"{pageSummary.PageSlug}\">Change</a><span class=\"govuk-visually-hidden\">{answer.Key}</span</dd>");
                    }
                    htmlContent.AppendHtmlLine("</div>");
                }
                htmlContent.AppendHtmlLine("</dl>");

            }
           
            using (var writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                return Task.FromResult(writer.ToString());
            }
            
        }
    }
}

