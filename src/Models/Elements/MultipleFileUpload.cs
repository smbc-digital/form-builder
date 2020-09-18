using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class MultipleFileUpload : Element
    {
        public MultipleFileUpload()
        {
            Type = EElementType.MultipleFileUpload;
        }

        public override string QuestionId => $"{base.QuestionId}-fileupload";
        public List<object> FileUpload { get; set; }
        public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            var allowedFileType = Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

            var properties = new Dictionary<string, dynamic>
            {
                { "name", QuestionId },
                { "id", QuestionId },
                { "type", "file" },
                //{ "files", "" },
                { "multiple", true },
                { "accept", string.Join(',', allowedFileType)}
            };

            if (DisplayAriaDescribedby)
                properties.Add("aria-describedby", GetDescribedByAttributeValue());

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
            Dictionary<string, dynamic> answers,
            List<object> results = null)
        {
            FileUpload = results ?? new List<object>();
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}