using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;

namespace form_builder.Models.Elements
{
    public class Button : Element
    {
        public Button()
        {
            Type = EElementType.Button;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var viewData = new Dictionary<string, dynamic> { { "displayAnchor", !CheckForStartPageSlug(formSchema, page) }, { "showSpinner", ShowSpinner(page.Behaviours, page.Elements, viewModel) } };

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
            bool isStreetAddress = element.Any(_ => _.Type == EElementType.Address || _.Type == EElementType.Street);

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
    }
}