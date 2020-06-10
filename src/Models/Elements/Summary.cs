using form_builder.Cache;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Builders;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;
using Microsoft.Extensions.DependencyInjection;
using form_builder.Mappers;

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
                htmlContent.AppendHtml($"<h2 class=\"govuk-heading-m\">{pageSummary.PageTitle}</h2>");
                foreach (var answer in pageSummary.Answers)
                {
                    htmlContent.AppendHtmlLine("<div class=\"govuk-summary-list__row\">");
                    htmlContent.AppendHtmlLine($"<dt class=\"govuk-summary-list__key\">{answer.Key}</dt>");
                    htmlContent.AppendHtmlLine($"<dd class=\"govuk-summary-list__value\">{answer.Value}</dd>");
                    htmlContent.AppendHtmlLine($"<dd class=\"govuk-summary-list__actions\"><a href=\"{pageSummary.PageSlug}\">Change <span class=\"govuk-visually-hidden\">{answer.Key}</span></a></dd>");
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

