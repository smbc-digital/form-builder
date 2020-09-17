using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class Button : Element
    {
        public bool DisplayButton { get; set; } = true;
        public Button()
        {
            Type = EElementType.Button;
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
            Properties.Text = GetButtonText(page.Elements, viewModel, page);

            if(!Properties.DisableOnClick)
                Properties.DisableOnClick = DisableIfSubmitOrLookup(page.Behaviours, page.Elements, viewModel);

            if (page.Elements.Any(_ => _.Type.Equals(EElementType.MultipleFileUpload)) && string.IsNullOrEmpty(viewModel[FileUploadConstants.SubPathViewModelKey]))
            {
                DisplayButton = false;
            }

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
                return SystemConstants.AddressSearchButtonText;

            if (element.Any(_ => _.Type == EElementType.Street) && viewModel.IsInitial())
                return SystemConstants.StreetSearchButtonText;

            if (element.Any(_ => _.Type == EElementType.Organisation) && viewModel.IsInitial())
                return SystemConstants.OrganisationSearchButtonText;

            if (page.Behaviours.Any(_ => _.BehaviourType == EBehaviourType.SubmitForm || _.BehaviourType == EBehaviourType.SubmitAndPay))
                return string.IsNullOrEmpty(Properties.Text) ? SystemConstants.SubmitButtonText : Properties.Text;

            return string.IsNullOrEmpty(Properties.Text) ? SystemConstants.NextStepButtonText : Properties.Text;
        }
    }
}