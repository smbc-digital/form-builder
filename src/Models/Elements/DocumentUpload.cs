using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace form_builder.Models.Elements
{
    public class DocumentUpload : Element
    {
        public DocumentUpload()
        {
            Type = EElementType.DocumentUpload;
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
            Properties.Text ??= ButtonConstants.DOCUMENT_UPLOAD_TEXT;

            Properties.DocumentUploadUrl = elementHelper.GenerateDocumentUploadUrl(this, formSchema, formAnswers);

            return viewRender.RenderAsync("DocumentUpload", this);
        }
    }
}
