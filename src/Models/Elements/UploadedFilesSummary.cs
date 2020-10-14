using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace form_builder.Models.Elements
{
    public class UploadedFilesSummary : Element
    {
        public UploadedFilesSummary()
        {
            Type = EElementType.UploadedFilesSummary;
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
            var currentMultiFileUpload = page.Elements.First(_ => _.Type.Equals(EElementType.MultipleFileUpload));

            var fileUploadElements = formSchema.Pages
                .SelectMany(_ => _.ValidatableElements)
                .Where(_ => _.Type == EElementType.MultipleFileUpload && _.Properties.QuestionId != currentMultiFileUpload.Properties.QuestionId)
                .Select(_ => _.Properties.QuestionId)
                .ToList();
                
            if(fileUploadElements.Any()) 
            {
                Properties.ClassName = "";
                fileUploadElements.ForEach((questionId) => {
                    var model = elementHelper.CurrentValue<JArray>(questionId, viewModel, formAnswers, FileUploadConstants.SUFFIX);

                    if(model != null && model.Any()){
                        List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(model.ToString());
                        Properties.ListItems.AddRange(response.Select(_ => _.TrustedOriginalFileName));
                    }
                });
            }

            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}