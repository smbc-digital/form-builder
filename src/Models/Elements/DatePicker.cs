﻿using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class DatePicker : Element
    {

        public DatePicker()
        {
            Type = EElementType.DatePicker;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            Properties.Date = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, string.Empty);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}
