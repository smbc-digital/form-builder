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
                    option.ConditionalElement.SetUpElementValue(elementHelper, viewModel,formAnswers);        
                }
            }

            // Check radio element has condition element
            // If doesn't -- do nothing
            // If it does -- loop round options
            // foreach option that has condition elelemt -- get current value if there is one
            // set properties.value to current value of option

            return await viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}