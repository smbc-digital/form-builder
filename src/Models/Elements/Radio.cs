using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class Radio : Element
    {
        public Radio()
        {
            Type = EElementType.Radio;
        }
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

            if (Properties.Options.Any(_ =>_.HasConditionalElement)) {
                foreach (Option option in Properties.Options) {
                    if (option.HasConditionalElement) {
                        option.ConditionalElement.SetUpElementValue(elementHelper, viewModel, formAnswers);
                    }           
                }
            }

            return await viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}