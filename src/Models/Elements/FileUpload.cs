using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;

namespace form_builder.Models.Elements
{
    public class FileUpload : Element
    {
        public FileUpload()
        {
            Type = EElementType.FileUpload;
        }

        public override string QuestionId => $"{base.QuestionId}-fileupload";

        public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            var allowedFileType = Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;
            var convertedMaxFileSize = Properties.MaxFileSize * 1024000;
            var appliedMaxFileSize = convertedMaxFileSize > 0 && convertedMaxFileSize < SystemConstants.DefaultMaxFileSize 
                                ? convertedMaxFileSize 
                                : SystemConstants.DefaultMaxFileSize;

            var properties = new Dictionary<string, dynamic>()
            {
                { "name", QuestionId },
                { "id", QuestionId },
                { "type", "file" },
                { "accept", string.Join(',', allowedFileType)},
                { "max-file-size", appliedMaxFileSize },
                { "onchange", "window.SMBCFrontend.ValidateSize(this)" }
            };

            if (DisplayAriaDescribedby)
            {
                properties.Add("aria-describedby", GetDescribedByAttributeValue());
            }

            return properties;
        }

        public override Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            List<object> results = null)
        {
            elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, string.Empty);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}
