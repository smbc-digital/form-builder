using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
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

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
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

                var optionsList = new List<SelectListItem>{ new SelectListItem($"{organisationResults.Count} organisations found", string.Empty)};
                organisationResults.ForEach((searchResult) => {
                    optionsList.Add(new SelectListItem(searchResult.Name, $"{searchResult.Reference}|{searchResult.Name}"));
                });

                var returnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
                return await viewRender.RenderAsync("OrganisationSelect", new Tuple<ElementViewModel, List<SelectListItem>>(new ElementViewModel{Element = this, ReturnURL = returnURL }, optionsList));
            }

            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-organisation-searchterm");

            var viewElement = new ElementViewModel
            {
                Element = this
            };

            return await viewRender.RenderAsync("OrganisationSearch", viewElement);
        }

        private string OrganisationSelectDescribeByValue(){
            var describedByValue = string.Empty;

            if (!string.IsNullOrEmpty(Properties.SelectHint))
            {
                describedByValue += $"{Properties.QuestionId}-organisation-hint ";
            }

            if (!IsValid)
            {
                describedByValue += $"{Properties.QuestionId}-organisation-error";
            }

            return describedByValue.Trim();
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type)
        {
            var properties = new Dictionary<string, dynamic>();

            switch (type)
            {
                case "Select":
                    properties.Add("id", $"{Properties.QuestionId}-organisation");
                    properties.Add("name", $"{Properties.QuestionId}-organisation");

                    if (!string.IsNullOrWhiteSpace(Properties.SelectHint) || !IsValid)
                    {
                        properties.Add("aria-describedby", OrganisationSelectDescribeByValue());
                    }

                return properties;
                default:
                    properties.Add("id", $"{Properties.QuestionId}-organisation-searchterm");
                    properties.Add("maxlength", Properties.MaxLength);

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue("-organisation-searchterm"));
                    }

                return properties;

            }
        }
    }
}