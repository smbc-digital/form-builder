using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class DocumentUpload : Element
    {
        public DocumentUpload() => Type = EElementType.DocumentUpload;

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

            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}
