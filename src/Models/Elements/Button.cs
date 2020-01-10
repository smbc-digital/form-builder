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

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var viewData = new Dictionary<string, object> { { "displayAnchor", !CheckForStartPageSlug(formSchema, page) }, { "behaviourType", CheckForBehaviour(page.Behaviours) }, { "address", CheckForAddressElement(page.Elements) }, { "street", CheckForStreetElement(page.Elements) } };

            return viewRender.RenderAsync("Button", this, viewData);
        }

        private bool CheckForStartPageSlug(FormSchema form, Page page)
        {
            return form.StartPageSlug == page.PageSlug;
        }


        private bool CheckForBehaviour(List<Behaviour> behaviour)
        {
            return behaviour.Any(_ => _.BehaviourType == EBehaviourType.SubmitForm);
        }

        private bool CheckForAddressElement(List<IElement> element)
        {
            return element.Any(_ => _.Type == EElementType.Address);
        }

        private bool CheckForStreetElement(List<IElement> element)
        {
            return element.Any(_ => _.Type == EElementType.Street);
        }
    }
}

