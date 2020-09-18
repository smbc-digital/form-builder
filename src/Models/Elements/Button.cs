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

namespace form_builder.Models.Elements
{
    public class Button : Element
    {
        public bool DisplaySubmitDataButton { get; set; } = true;

        public bool DisplayNonDataSubmitButton { get; set; } = false;

        public List<Condition> Conditions { get; set; }

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
            Dictionary<string, dynamic> answers,
            List<object> results = null
            )
        {
            Properties.Text = GetButtonText(page.Elements, viewModel, page);

            if (Conditions != null)
            {
                DisplaySubmitDataButton = CheckButtonMeetsConditions(answers);

                if (DisplaySubmitDataButton && page.Elements.Any(_ => _.Type.Equals(EElementType.MultipleFileUpload)))
                {
                    DisplayNonDataSubmitButton = true;
                    DisplaySubmitDataButton = false;
                }
            }

            if(!Properties.DisableOnClick)
                Properties.DisableOnClick = DisableIfSubmitOrLookup(page.Behaviours, page.Elements, viewModel);

            return viewRender.RenderAsync("Button", this);
        }

        private bool CheckButtonMeetsConditions(Dictionary<string, dynamic> answers)
        {
            var conditionValidator = new ConditionValidator();

            return Conditions.Count == 0 ||
                   Conditions.All(condition => conditionValidator.IsValid(condition, answers));
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