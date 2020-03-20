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

namespace form_builder.Models.Elements
{
    public class Street : Element
    {
        public Street()
        {
            Type = EElementType.Street;
        }
        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);

            var streetKey = $"{Properties.QuestionId}-street";

            if (viewModel.ContainsKey("StreetStatus") && viewModel["StreetStatus"] == "Select" || viewModel.ContainsKey(streetKey) && !string.IsNullOrEmpty(viewModel[streetKey]))
            {
                Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
                if (string.IsNullOrEmpty(Properties.Value))
                {
                    if ((string)viewModel["StreetStatus"] == "Select")
                    {
                        var streetaddress = $"{Properties.QuestionId}-streetaddress";
                        Properties.Value = (string)viewModel[streetaddress];
                        if(string.IsNullOrEmpty(Properties.Value))
                        {
                            Properties.Value = (string)viewModel[streetKey];
                        }
                    }
                    else
                    {
                        Properties.Value = (string)viewModel[streetKey];
                    }

                }
                var url = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

                var viewElement = new ElementViewModel
                {
                    Element = this,
                    ReturnURL = url
                };

                return await viewRender.RenderAsync("StreetSelect", new Tuple<ElementViewModel, List<AddressSearchResult>>(viewElement, addressSearchResults));
            }

            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-street");
            return await viewRender.RenderAsync("StreetSearch", this);
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type)
        {
            var properties = new Dictionary<string, dynamic>()
            {
                { "id", $"{Properties.QuestionId}-street" },
                { "maxlength", Properties.MaxLength }
            };

            if (DisplayAriaDescribedby)
            {
                properties.Add("aria-describedby", DescribedByValue("-street"));
            }

            return properties;
        }
    }
}