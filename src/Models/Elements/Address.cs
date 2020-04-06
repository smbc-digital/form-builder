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
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace form_builder.Models.Elements
{
    public class Address : Element
    {
        public Address()
        {
            Type = EElementType.Address;          
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var postcodeKey = $"{Properties.QuestionId}-postcode";
            
            var viewElement = new ElementViewModel
            {
                Element = this,                
            };

            if(!IsValid && viewModel.ContainsKey("AddressStatus") && viewModel["AddressStatus"] == "Search")
            {
                Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");
                return await viewRender.RenderAsync("AddressSearch", viewElement);
            }

            if (viewModel.ContainsKey("AddressStatus") && viewModel["AddressStatus"] == "Select" || viewModel.ContainsKey(postcodeKey) && !string.IsNullOrEmpty(viewModel[postcodeKey]))
            {
                Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");                 
                viewElement.ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
                viewElement.ManualAddressURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}/manual";

                var optionsList = new List<SelectListItem>{ new SelectListItem($"{addressSearchResults.Count} addresses found", string.Empty)};
                addressSearchResults.ForEach((searchResult) => {
                    optionsList.Add(new SelectListItem(searchResult.Name, $"{searchResult.UniqueId}|{searchResult.Name}"));
                });
                
                return await viewRender.RenderAsync("AddressSelect", new Tuple<ElementViewModel, List<SelectListItem>>(viewElement, optionsList));
            }

            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");
            return await viewRender.RenderAsync("AddressSearch", viewElement);
        }

        private string AddressSelectDescribeByValue(){
            var describedByValue = string.Empty;

            if (!string.IsNullOrEmpty(Properties.SelectHint))
            {
                describedByValue += $"{Properties.QuestionId}-address-hint ";
            }

            if (!IsValid)
            {
                describedByValue += $"{Properties.QuestionId}-address-error";
            }

            return describedByValue.Trim();
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type)
        {
            var properties = new Dictionary<string, dynamic>();
            switch (type)
            {
                case "Select":
                    properties.Add("id", $"{Properties.QuestionId}-address");
                    properties.Add("name", $"{Properties.QuestionId}-address");

                    if (!string.IsNullOrWhiteSpace(Properties.SelectHint) || !IsValid)
                    {
                        properties.Add("aria-describedby", AddressSelectDescribeByValue());
                    }

                    return properties;
                default:
                    properties.Add("id", $"{Properties.QuestionId}-postcode");

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", GetDescribedByAttributeValue("-postcode"));
                    }

                    return properties;
                }
        }

        public Task RenderAsync(IViewRender object1, IElementHelper object2, string v, List<SelectListItem> list1, List<OrganisationSearchResult> list2, Dictionary<string, dynamic> viewModel, Page page, FormSchema schema, IHostingEnvironment object3)
        {
            throw new NotImplementedException();
        }
    }
}
