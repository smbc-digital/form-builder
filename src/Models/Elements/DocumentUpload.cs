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
            IHttpContextAccessor httpContextAccessor,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            Properties.Text ??= ButtonConstants.DOCUMENT_UPLOAD_TEXT;

            var caseRefInBytes = Encoding.ASCII.GetBytes(formAnswers.CaseReference);
            var urlOrigin = $"https://{httpContextAccessor.HttpContext.Request.Host}/";
            var urlPath = $"{formSchema.BaseURL}/{FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH}{SystemConstants.CaseReferenceQueryString}{Convert.ToBase64String(caseRefInBytes)}";
            Properties.DocumentUploadUrl = environment.EnvironmentName.Equals("local")
                ? $"{urlOrigin}{urlPath}"
                : $"{urlOrigin}v2/{urlPath}";

            return viewRender.RenderAsync("DocumentUpload", this);
        }
    }
}
