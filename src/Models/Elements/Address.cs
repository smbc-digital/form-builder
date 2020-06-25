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
using Newtonsoft.Json.Linq;
using System.Linq;
using form_builder.Constants;
using form_builder.Enum;

namespace form_builder.Models.Elements
{
    public class Address : Element
    {
        public Address()
        {
            Type = EElementType.Address;
        }

        public override async Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IHostingEnvironment environment,
            List<object> results = null)
        {
            var viewElement = new ElementViewModel
            {
                Element = this,
            };

            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);
            switch (subPath as string) {
                case LookUpConstants.Manual:
                    Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");

                    SetAddressProperties(viewModel, Properties.Value);

                    if (results != null && results.Count == 0)
                        Properties.DisplayNoResultsIAG = true;

                    viewElement = new ElementViewModel
                    {
                        Element = this,
                        ManualAddressURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}"
                    };

                    return await viewRender.RenderAsync("AddressManual", viewElement);

                case LookUpConstants.Automatic:
                    Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");

                    viewElement.ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
                    viewElement.ManualAddressURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}/{LookUpConstants.Manual}";

                    var selectedAddress = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-address");
                    var optionsList = new List<SelectListItem> { new SelectListItem($"{results.Count} addresses found", string.Empty) };
                    results.ForEach((objectResult) => {
                        AddressSearchResult searchResult;

                        if ((objectResult as JObject) != null)
                        {
                            searchResult = (objectResult as JObject).ToObject<AddressSearchResult>();
                        }
                        else
                        {
                            searchResult = objectResult as AddressSearchResult;
                        }

                        optionsList.Add(new SelectListItem(
                            searchResult.Name,
                            $"{searchResult.UniqueId}|{searchResult.Name}", searchResult.UniqueId.Equals(selectedAddress)));
                    });

                    return await viewRender.RenderAsync("AddressSelect", new Tuple<ElementViewModel, List<SelectListItem>>(viewElement, optionsList));

                default:
                    Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");
                    return await viewRender.RenderAsync("AddressSearch", viewElement);
            }
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
                        properties.Add("aria-describedby", DescribedByValue("-postcode"));
                    }

                    return properties;
                }
        }
        
        public override string GetLabelText(){
            var optionalLabelText = Properties.Optional ? " (optional)" : string.Empty;
            
            return $"{Properties.AddressLabel}{optionalLabelText}";
        }

        private void SetAddressProperties(Dictionary<string, dynamic> viewModel, string searchTerm)
        {
            Properties.AddressManualAddressLine1 = viewModel.FirstOrDefault(_ => _.Key.Contains("AddressManualAddressLine1")).Value;
            Properties.AddressManualAddressLine2 = viewModel.FirstOrDefault(_ => _.Key.Contains("AddressManualAddressLine2")).Value;
            Properties.AddressManualAddressTown = viewModel.FirstOrDefault(_ => _.Key.Contains("AddressManualAddressTown")).Value;
            Properties.AddressManualAddressPostcode = viewModel.FirstOrDefault(_ => _.Key.Contains("AddressManualAddressPostcode")).Value ?? searchTerm;
        }

        public override string GenerateFieldsetProperties()
        {
            if (!string.IsNullOrWhiteSpace(Properties.AddressManualHint))
            {
                return $"aria-describedby = {Properties.QuestionId}-hint";
            }

            return string.Empty;
        }
    }
}
