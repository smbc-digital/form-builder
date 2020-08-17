using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using form_builder.Constants;
using Newtonsoft.Json.Linq;

namespace form_builder.Models.Elements
{
    public class Organisation : Element
    {
        public Organisation()
        {
            Type = EElementType.Organisation;
        }

        public override async Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IHostingEnvironment environment,
            List<object> results = null)
        {
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);

            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

            switch (subPath as string)
            {
                case LookUpConstants.Automatic:
                    Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-organisation-searchterm");

                    var url = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

                    var viewElement = new ElementViewModel
                    {
                        Element = this,
                        ReturnURL = url
                    };

                    var selectedOrganisation = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-organisation");
                    var optionsList = new List<SelectListItem> { new SelectListItem($"{results?.Count} organisations found", string.Empty) };
                    results?.ForEach((objectResult) =>
                    {
                        OrganisationSearchResult searchResult;

                        if ((objectResult as JObject) != null)
                            searchResult = (objectResult as JObject).ToObject<OrganisationSearchResult>();
                        else
                            searchResult = objectResult as OrganisationSearchResult;

                        optionsList.Add(new SelectListItem(searchResult.Name, $"{searchResult.Reference}|{searchResult.Name}", searchResult.Reference.Equals(selectedOrganisation)));
                    });

                    return await viewRender.RenderAsync("OrganisationSelect", new Tuple<ElementViewModel, List<SelectListItem>>(viewElement, optionsList));
                default:
                    Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-organisation-searchterm");
                    var test = await viewRender.RenderAsync("OrganisationSearch", new ElementViewModel { Element = this });
                    return test;
            }
        }

        private string OrganisationSelectDescribeByValue()
        {
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

        public override string GetLabelText()
        {
            var optionalLabelText = Properties.Optional ? " (optional)" : string.Empty;

            return string.IsNullOrEmpty(Properties.OrganisationLabel)
            ? $"Search for an organisation{optionalLabelText}"
            : $"{Properties.OrganisationLabel}{optionalLabelText}";
        }
    }
}