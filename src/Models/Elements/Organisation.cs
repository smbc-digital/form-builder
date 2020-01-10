using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class Organisation : Element
    {
        public Organisation()
        {
            Type = EElementType.Organisation;
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var organisationKey = $"{Properties.QuestionId}-organisation-searchterm";

            if (viewModel.ContainsKey("OrganisationStatus") && viewModel["OrganisationStatus"] == "Select" || viewModel.ContainsKey(organisationKey) && !string.IsNullOrEmpty(viewModel[organisationKey]))
            {
                Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
                if (string.IsNullOrEmpty(Properties.Value))
                {
                    if (viewModel["OrganisationStatus"] == "Select")
                    {
                        var organisation = $"{Properties.QuestionId}-organisation";
                        Properties.Value = viewModel[organisation];
                        if (string.IsNullOrEmpty(Properties.Value))
                        {
                            Properties.Value = viewModel[organisationKey];
                        }
                    }
                    else
                    {
                        Properties.Value = viewModel[organisationKey];
                    }
                }

                var returnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
                return await viewRender.RenderAsync("OrganisationSelect", new Tuple<ElementViewModel, List<OrganisationSearchResult>>(new ElementViewModel{Element = this, ReturnURL = returnURL }, organisationResults));
            }

            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-organisation-searchterm");

            var viewElement = new ElementViewModel
            {
                Element = this
            };

            return await viewRender.RenderAsync("OrganisationSearch", viewElement);
        }
    }
}