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

namespace form_builder.Models.Elements
{
    public class Street : Element
    {
        public const string SEARCH_QUESTION_SUFFIX = "-street";
        public List<SelectListItem> Items { get; set; }
        public string ReturnURL { get; set; }
        public string StreetSearchQuestionId => $"{Properties.QuestionId}{SEARCH_QUESTION_SUFFIX}";
        public string StreetSelectQuestionId => $"{Properties.QuestionId}";
        private bool IsSelect { get; set; } = false; 
        public override string  Hint => IsSelect ? Properties.SelectHint : base.Hint;
        public override string  QuestionId => IsSelect ? StreetSelectQuestionId : StreetSearchQuestionId;
        public string ChangeHeader => "Street";
        public override string Label
        {
            get
            {
                if(IsSelect)
                {
                    return string.IsNullOrEmpty(Properties.SelectLabel) ? "Street" : Properties.SelectLabel;
                }

                return string.IsNullOrEmpty(Properties.Label) ? "Search for a street" : Properties.Label;
            }
        }
        public Street()
        {
            Type = EElementType.Street;
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> searchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            IsSelect = viewModel.ContainsKey("StreetStatus") && viewModel["StreetStatus"] == "Select" || viewModel.ContainsKey(StreetSearchQuestionId) && !string.IsNullOrEmpty(viewModel[StreetSearchQuestionId]);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);
            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, SEARCH_QUESTION_SUFFIX);


            if (IsSelect)
            {
                Items = new List<SelectListItem>{ new SelectListItem($"{searchResults.Count} streets found", string.Empty)};
                searchResults.ForEach((_) => { Items.Add(new SelectListItem(_.Name, $"{_.UniqueId}|{_.Name}")); });
                ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

                if (string.IsNullOrEmpty(Properties.Value))
                {
                    Properties.Value = (string)viewModel[StreetSearchQuestionId];
                    if ((string)viewModel["StreetStatus"] == "Select")
                    {
                        Properties.Value = (string)viewModel[QuestionId];
                    }
                }

                return await viewRender.RenderAsync("StreetSelect", this);
            }

            return await viewRender.RenderAsync("StreetSearch", this);
        }


        public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            var elemnentProperties = new Dictionary<string, dynamic>();
            elemnentProperties.Add("id", $"{QuestionId}");
            elemnentProperties.Add("name", $"{QuestionId}");

            if (DisplayAriaDescribedby)
            {
                elemnentProperties.Add("aria-describedby", GetDescribedByAttributeValue());
            }

            if(string.IsNullOrEmpty(type))
            {
                elemnentProperties.Add("maxlength", Properties.MaxLength);
            }
 
            return elemnentProperties;
        }

        public override string GetLabelText(){
            var optionalLabelText = Properties.Optional ? " (optional)" : string.Empty;
            
            return string.IsNullOrEmpty(Properties.Label)
            ? $"Search for a street{optionalLabelText}"
            : $"{Properties.Label}{optionalLabelText}";
        }
    }
}