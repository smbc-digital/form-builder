using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class DocumentDownloadButton : Element
    {
        public DocumentDownloadButton() => Type = EElementType.DocumentDownloadButton;

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
            Properties.Text = string.IsNullOrEmpty(Properties.Text) ? $"Download {Properties.DocumentType} document" : Properties.Text;
            Properties.Source = $"/document/Summary/{Properties.DocumentType}/{guid}";
            return viewRender.RenderAsync("DocumentDownloadButton", this);
        }
    }
}