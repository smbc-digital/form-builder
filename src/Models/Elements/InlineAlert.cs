﻿using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class InlineAlert : Element
    {
        public InlineAlert()
        {
            Type = EElementType.InlineAlert;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid,List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment,
           List<object> results = null)
        {
            elementHelper.CheckIfLabelAndTextEmpty(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}
