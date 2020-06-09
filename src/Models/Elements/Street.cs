using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using form_builder.Constants;
using Newtonsoft.Json.Linq;

namespace form_builder.Models.Elements
{
    public class Street : Element
    {
        public Street()
        {
            Type = EElementType.Street;
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment,
           List<object> results = null)
        {
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);

            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);
            //var hasStreet = viewModel.TryGetValue($"{Properties.QuestionId}-street", out var streetValue);
            //if (hasStreet && string.IsNullOrEmpty(streetValue))
            //    subPath = "";

            switch (subPath as string)
            {
                case LookUpConstants.Automatic:
                    Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-street");
                    //if (string.IsNullOrEmpty(Properties.Value))
                    //{
                    //    Properties.Value = streetValue;
                    //}

                    var url = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

                    var viewElement = new ElementViewModel
                    {
                        Element = this,
                        ReturnURL = url
                    };

                    var optionsList = new List<SelectListItem> { new SelectListItem($"{results?.Count} addresses found", string.Empty) };
                    results?.ForEach((objectResult) => {
                        AddressSearchResult searchResult;

                        if ((objectResult as JObject) != null)
                            searchResult = (objectResult as JObject).ToObject<AddressSearchResult>();
                        else
                            searchResult = objectResult as AddressSearchResult;

                        optionsList.Add(new SelectListItem(searchResult.Name, $"{searchResult.UniqueId}|{searchResult.Name}"));
                    });

                    return await viewRender.RenderAsync("StreetSelect", new Tuple<ElementViewModel, List<SelectListItem>>(viewElement, optionsList));
                default:

                    Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-street");
                    return await viewRender.RenderAsync("StreetSearch", this);
            }
        }

        private string StreetSelectDescribeByValue(){
            var describedByValue = string.Empty;

            if (!string.IsNullOrEmpty(Properties.SelectHint))
            {
                describedByValue += $"{Properties.QuestionId}-streetaddress-hint ";
            }

            if (!IsValid)
            {
                describedByValue += $"{Properties.QuestionId}-streetaddress-error";
            }

            return describedByValue.Trim();
        }


        public override Dictionary<string, dynamic> GenerateElementProperties(string type)
        {
            var properties = new Dictionary<string, dynamic>();

            switch(type){
                case "Select":
                    properties.Add("id", $"{Properties.QuestionId}-streetaddress");
                    properties.Add("name", $"{Properties.QuestionId}-streetaddress");

                    if (!string.IsNullOrWhiteSpace(Properties.SelectHint) || !IsValid)
                    {
                        properties.Add("aria-describedby", StreetSelectDescribeByValue());
                    }

                    return properties;

                default:
                    properties.Add("id", $"{Properties.QuestionId}-street");
                    properties.Add("maxlength", Properties.MaxLength);

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue("-street"));
                    }

                    return properties;
            }
        }

        public override string GetLabelText(){
            var optionalLabelText = Properties.Optional ? " (optional)" : string.Empty;
            
            return string.IsNullOrEmpty(Properties.StreetLabel)
            ? $"Search for a street{optionalLabelText}"
            : $"{Properties.StreetLabel}{optionalLabelText}";
        }
    }
}