using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace form_builder.Models.Elements
{
    public class Button : Element
    {
        public Button()
        {
            Type = EElementType.Button;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var viewData = new Dictionary<string, object> { { "displayAnchor", !CheckForStartPageSlug(formSchema, page) }, { "bType", CheckForBehaviour(page.Behaviours) } };

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
    }
}

