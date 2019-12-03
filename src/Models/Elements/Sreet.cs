using form_builder.Enum;
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
    public class Street : Element, IElement
    {
        public Street()
        {
            Type = EElementType.Street;
        }
        public new async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<StockportGovUK.NetStandard.Models.Models.Verint.Street> streetSearchResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var streetKey = $"{Properties.QuestionId}-street";

            if (viewModel.ContainsKey("StreetStatus") && viewModel["StreetStatus"] == "Select" || viewModel.ContainsKey(streetKey) && !string.IsNullOrEmpty(viewModel[streetKey]))
            {
                Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
                return await viewRender.RenderAsync("StreetSelect", new Tuple<Element, List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>(this, streetSearchResults));
            }

            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
            return await viewRender.RenderAsync("StreetSearch", this);
        }
    }
}
