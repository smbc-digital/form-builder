using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class Radio : Element
    {
        public Radio()
        {
            Type = EElementType.Radio;
        }

        public override ErrorViewModel GetErrorViewModel() => new ErrorViewModel { Id = $"{QuestionId}-0", IsValid = IsValid, Message = ValidationMessage };

        public async override Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckForRadioOptions(this);
            elementHelper.ReCheckPreviousRadioOptions(this);
            return await viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}