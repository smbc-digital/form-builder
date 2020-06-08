using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using form_builder.Constants;

namespace form_builder.Models.Elements
{
    public class Button : Element
    {
        public Button()
        {
            Type = EElementType.Button;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment,
            List<object> results = null)
        {
            var viewData = new Dictionary<string, dynamic> { { "displayAnchor", !CheckForStartPageSlug(formSchema, page) }, { "showSpinner", ShowSpinner(page.Behaviours, page.Elements, viewModel) }, { "buttonText", GetButtonText(page.Elements, viewModel, page) } };

            return viewRender.RenderAsync("Button", this, viewData);
        }

        private bool CheckForStartPageSlug(FormSchema form, Page page)
        {
            return form.StartPageSlug == page.PageSlug;
        }

        private bool CheckForBehaviour(List<Behaviour> behaviour)
        {
            return behaviour.Any(_ => _.BehaviourType == EBehaviourType.SubmitForm || _.BehaviourType == EBehaviourType.SubmitAndPay);
        }

        private bool CheckForStreetAddress(List<IElement> element, Dictionary<string, dynamic> viewModel)
        {
            var isStreetAddress = element.Any(_ => _.Type == EElementType.Address || _.Type == EElementType.Street);

            if (isStreetAddress && (viewModel.ContainsKey("AddressStatus") || viewModel.ContainsKey("StreetStatus")))
            {
                return false;
            }
           
            return isStreetAddress;
        }

        private bool ShowSpinner(List<Behaviour> behaviour, List<IElement> element, Dictionary<string, dynamic> viewModel)
        {
            if (CheckForBehaviour(behaviour) || CheckForStreetAddress(element, viewModel))
            {
                return true;
            }

            return false;
        }

        private string GetButtonText(List<IElement> element, Dictionary<string, dynamic> viewModel, Page page)
        {
            var containsAddressElement = element.Any(_ => _.Type == EElementType.Address);

            if (containsAddressElement && viewModel.ContainsKey("subPath") && viewModel["subPath"] == string.Empty)
            {
                return SystemConstants.AddressSearchButtonText;
            }

            return string.IsNullOrEmpty(Properties.Text) ? SystemConstants.NextStepButtonText : Properties.Text;
        }
    }
}