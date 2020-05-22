using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using form_builder.Constants;

namespace form_builder.Models.Elements
{
    public class Street : Element
    {
        public List<SelectListItem> Items { get; set; }
        public string ReturnURL { get; set; }
        public string StreetSearchQuestionId => $"{Properties.QuestionId}";
        public string StreetSelectQuestionId => $"{Properties.QuestionId}{StreetConstants.SELECT_SUFFIX}";
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

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> searchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> answers, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            IsSelect = answers.ContainsKey("StreetStatus") && answers["StreetStatus"] == "Select" || answers.ContainsKey(StreetSearchQuestionId) && !string.IsNullOrEmpty(answers[StreetSearchQuestionId]);
            Properties.Value = elementHelper.CurrentValue(this, answers, page.PageSlug, guid);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);

            if (!IsSelect)
            {
                return await viewRender.RenderAsync("StreetSearch", this);
            }

            Items = new List<SelectListItem>{ new SelectListItem($"{searchResults.Count} streets found", string.Empty)};
            searchResults.ForEach((_) => { Items.Add(new SelectListItem(_.Name, $"{_.UniqueId}|{_.Name}")); });
            ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
            return await viewRender.RenderAsync("StreetSelect", this);
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