using form_builder.Cache;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;
using Microsoft.Extensions.DependencyInjection;

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
            var formData = elementHelper.GetFormData(guid);

            var htmlContent = new HtmlContentBuilder();

            foreach(var currentPage in formData.Pages)
            {
                htmlContent.AppendHtmlLine("<dl class=\"govuk-summary-list govuk-!-margin-bottom-9\">");
                foreach (var answer in currentPage.Answers)
                {
                    if (!answer.QuestionId.EndsWith("address"))
                    {
                        htmlContent.AppendHtmlLine("<div class=\"govuk-summary-list__row\">");
                        htmlContent.AppendHtmlLine($"<dt class=\"govuk-summary-list__key\">{answer.QuestionId}</dt>");
                        htmlContent.AppendHtmlLine($"<dd class=\"govuk-summary-list__value\">{answer.Response}</dd>");
                        htmlContent.AppendHtmlLine($"<dd class=\"govuk-summary-list__value\"><a href=\"{currentPage.PageSlug}\">Change Value</a></dd>");
                        htmlContent.AppendHtmlLine("</div>");
                    }
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

