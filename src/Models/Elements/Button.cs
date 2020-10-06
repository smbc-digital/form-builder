using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Conditions;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace form_builder.Models.Elements
{
    public class Button : Element
    {
        public Button()
        {
            Type = EElementType.Button;
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
            Properties.Text = GetButtonText(page.Elements, viewModel, page);          

            if(!Properties.DisableOnClick)
                Properties.DisableOnClick = DisableIfSubmitOrLookup(page.Behaviours, page.Elements, viewModel);

            return viewRender.RenderAsync("Button", this);
        }

        private bool CheckForBehaviour(List<Behaviour> behaviour)
        {
            return behaviour.Any(_ => _.BehaviourType == EBehaviourType.SubmitForm || _.BehaviourType == EBehaviourType.SubmitAndPay);
        }

        private bool CheckForLookups(List<IElement> element, Dictionary<string, dynamic> viewModel)
        {
            var containsLookupElement = element.Any(_ => _.Type == EElementType.Address || _.Type == EElementType.Street  || _.Type == EElementType.Organisation);

            if (containsLookupElement && !viewModel.IsInitial())
                return false;
                
            return containsLookupElement;
        }

        private bool DisableIfSubmitOrLookup(List<Behaviour> behaviour, List<IElement> element, Dictionary<string, dynamic> viewModel)
        {
             return CheckForBehaviour(behaviour) || CheckForLookups(element, viewModel);
        }

        private string GetButtonText(List<IElement> element, Dictionary<string, dynamic> viewModel, Page page)
        {
            if (element.Any(_ => _.Type == EElementType.Address) && viewModel.IsInitial())
                return ButtonConstants.ADDRESS_SEARCH_TEXT;

            if (element.Any(_ => _.Type == EElementType.Street) && viewModel.IsInitial())
                return ButtonConstants.STREET_SEARCH_TEXT;

            if (element.Any(_ => _.Type == EElementType.Organisation) && viewModel.IsInitial())
                return ButtonConstants.ORG_SEARCH_TEXT;

            if (page.Behaviours.Any(_ => _.BehaviourType == EBehaviourType.SubmitForm || _.BehaviourType == EBehaviourType.SubmitAndPay))
                return string.IsNullOrEmpty(Properties.Text) ? ButtonConstants.SUBMIT_TEXT : Properties.Text;

            return string.IsNullOrEmpty(Properties.Text) ? ButtonConstants.NEXTSTEP_TEXT : Properties.Text;
        }
    }
}