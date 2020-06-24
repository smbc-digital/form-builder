using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Services.PayService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Models.Elements
{
    public class PaymentSummary : Element
    {
        public PaymentSummary()
        {
            Type = EElementType.PaymentSummary;
        }

        public override Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IHostingEnvironment environment,
            List<object> results = null)
        {
            var htmlContent = new HtmlContentBuilder();

            var paymentSummaryElement = page.Elements.FirstOrDefault(_ => _.Type == EElementType.PaymentSummary);

            htmlContent.AppendHtmlLine($"<p class=\"smbc-body\">The cost is &pound{paymentSummaryElement.Properties.PaymentAmount}</p>");
            htmlContent.AppendHtmlLine("<p class=\"smbc-body\">Use the button below to continue to our payments page where you&#39;ll need your credit or debit card details.</p>");
            
            using (var writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                return Task.FromResult(writer.ToString());
            }
        }
    }
}
