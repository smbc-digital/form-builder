﻿using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
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

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var streetKey = $"{Properties.QuestionId}-organisation-searchterm";

            if (viewModel.ContainsKey("OrganisationStatus") && viewModel["OrganisationStatus"] == "Select" || viewModel.ContainsKey(streetKey) && !string.IsNullOrEmpty(viewModel[streetKey]))
            {
                Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
                if (string.IsNullOrEmpty(Properties.Value))
                {
                    if (viewModel["OrganisationStatus"] == "Select")
                    {
                        var streetaddress = $"{Properties.QuestionId}-organisation";
                        Properties.Value = viewModel[streetaddress];
                        if (string.IsNullOrEmpty(Properties.Value))
                        {
                            Properties.Value = viewModel[streetKey];
                        }
                    }
                    else
                    {
                        Properties.Value = viewModel[streetKey];
                    }
                }

                return await viewRender.RenderAsync("OrganisationSelect", new Tuple<ElementViewModel, List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation>>(new ElementViewModel{Element = this}, organisationResults));
            }

            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);

            var viewElement = new ElementViewModel
            {
                Element = this
            };

            return await viewRender.RenderAsync("OrganisationSearch", viewElement);
        }
    }
}