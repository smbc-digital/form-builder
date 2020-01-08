﻿using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Extensions;

namespace form_builder.Models.Elements
{
    public class Address : Element
    {
        public Address()
        {
            Type = EElementType.Address;
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var postcodeKey = $"{Properties.QuestionId}-postcode";
            var viewElement = new ElementViewModel
            {
                Element = this,
            };

            if (viewModel.ContainsKey("AddressStatus") && viewModel["AddressStatus"] == "Select" || viewModel.ContainsKey(postcodeKey) && !string.IsNullOrEmpty(viewModel[postcodeKey]))
            {
                Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
                
                viewElement.ManualAddressURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}/manual";
                viewElement.ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

                return await viewRender.RenderAsync("AddressSelect", new Tuple<ElementViewModel, List<AddressSearchResult>>(viewElement, addressSearchResults));
            }

            viewElement.ManualAddressURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}/manual";

            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");
            return await viewRender.RenderAsync("AddressSearch", viewElement);
        }
    }
}
