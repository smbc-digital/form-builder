using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace form_builder.Models.Elements
{
    public class MultipleFileUpload : Element
    {
        public MultipleFileUpload()
        {
            Type = EElementType.MultipleFileUpload;
        }

        public string AllowFileTypeText { get { return Properties.AllowedFileTypes?.ToReadableFileType() ?? SystemConstants.AcceptedMimeTypes.ToReadableFileType();} }
        public string MaxFileSizeText { get { return $"{(Properties.MaxFileSize * 1024000 == 0 ? SystemConstants.DefaultMaxFileSize.ToReadableMaxFileSize() : Properties.MaxFileSize)}MB"; } }
        public string MaxCombinedFileSizeText { get { return $"{(Properties.MaxCombinedFileSize == 0 ? SystemConstants.DefaultMaxCombinedFileSize.ToReadableMaxFileSize() : Properties.MaxCombinedFileSize)}MB"; } }

        public override string QuestionId => $"{base.QuestionId}{FileUploadConstants.SUFFIX}";
        public List<string> CurrentFilesUploaded { get; set; } = new List<string>();
        public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            var allowedFileType = Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

            var properties = new Dictionary<string, dynamic>
            {
                { "name", QuestionId },
                { "id", QuestionId },
                { "type", "file" },
                { "multiple", true },
                { "accept", string.Join(',', allowedFileType)}
            };

            if (DisplayAriaDescribedby)
                properties.Add("aria-describedby", GetDescribedByAttributeValue());

            return properties;
        }
        public string SubmitButtonText;

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
            var currentAnswer = elementHelper.CurrentValue<JArray>(this, viewModel, page.PageSlug, guid, FileUploadConstants.SUFFIX);

            SubmitButtonText = SetSubmitButtonText(page);

            if(currentAnswer != null){
                List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(currentAnswer.ToString());
                CurrentFilesUploaded = response.Select(_ => _.TrustedOriginalFileName).ToList();
            }

            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);

            return viewRender.RenderAsync(Type.ToString(), this);
        }

        private string SetSubmitButtonText(Page page)
        {
            if (page.Behaviours.Any(_ => _.BehaviourType == EBehaviourType.SubmitForm || _.BehaviourType == EBehaviourType.SubmitAndPay))
                return string.IsNullOrEmpty(Properties.PageSubmitButtonLabel) ? SystemConstants.SubmitButtonText : Properties.PageSubmitButtonLabel;

            return string.IsNullOrEmpty(Properties.PageSubmitButtonLabel) ? SystemConstants.NextStepButtonText : Properties.PageSubmitButtonLabel;
        }
    }
}