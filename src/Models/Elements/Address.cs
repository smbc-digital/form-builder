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
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;

namespace form_builder.Models.Elements
{
    public class Address : Element
    {
        public Address()
        {
            Type = EElementType.Address;          
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            
            
            var postcodeKey = $"{Properties.QuestionId}-postcode";
            
            var viewElement = new ElementViewModel
            {
                Element = this,                
            };

            if(!IsValid && viewModel.ContainsKey("AddressStatus") && viewModel["AddressStatus"] == "Search")
            {
                viewElement.ManualAddressURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}/manual";

                Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");
                return await viewRender.RenderAsync("AddressSearch", viewElement);
            }
           
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

        public override Dictionary<string, object> GenerateElementProperties()
        {
            var properties = new Dictionary<string, object>()
            {
                { "id", $"{Properties.QuestionId}-postcode" },
                { "maxlength", Properties.MaxLength }
            };

            if (DisplayAriaDescribedby)
            {
                properties.Add("aria-describedby", DescribedByValue("-postcode"));
            }

            return properties;
        }
    }
}
