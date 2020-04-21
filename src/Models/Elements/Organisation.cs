using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class Organisation : Element
    {
        public const string SEARCH_QUESTION_SUFFIX = "-organisation-searchterm";
        public List<SelectListItem> Items { get; set; }
        public string ReturnURL { get; set; }
        public string OrganisationSearchQuestionId => $"{Properties.QuestionId}{SEARCH_QUESTION_SUFFIX}";
        public string OrganisationSelectQuestionId => $"{Properties.QuestionId}";
        private bool IsSelect { get; set; } = false; 
        public override string  Hint => IsSelect ? Properties.SelectHint : base.Hint;
        public override string  QuestionId => IsSelect ? OrganisationSelectQuestionId : OrganisationSearchQuestionId;
        public string ChangeHeader => "Organisation";
        public override string Label
        {
            get
            {
                if(IsSelect)
                {
                    return string.IsNullOrEmpty(Properties.SelectLabel) ? "Organisation" : Properties.SelectLabel;
                }

                return string.IsNullOrEmpty(Properties.Label) ? "Search for an organisation" : Properties.Label;
            }
        }

        public Organisation()
        {
            Type = EElementType.Organisation;
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> searchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> answers, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            IsSelect = answers.ContainsKey("OrganisationStatus") && answers["OrganisationStatus"] == "Select" || answers.ContainsKey(OrganisationSearchQuestionId) && !string.IsNullOrEmpty(answers[OrganisationSearchQuestionId]);
            Properties.Value = answers.ContainsKey(OrganisationSearchQuestionId) ? answers[OrganisationSearchQuestionId] : string.Empty;
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);

            if (IsSelect)
            {
                Items = new List<SelectListItem>{ new SelectListItem($"{organisationResults.Count} organisations found", string.Empty)};
                organisationResults.ForEach((_) => { Items.Add(new SelectListItem(_.Name, $"{_.Reference}|{_.Name}")); });
                ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
                
                if (string.IsNullOrEmpty(Properties.Value))
                {
                    Properties.Value = (string)answers[OrganisationSearchQuestionId];
                    if ((string)answers["OrganisationStatus"] == "Select")
                    {
                        Properties.Value = (string)answers[QuestionId];
                    }
                }

                return await viewRender.RenderAsync("OrganisationSelect", this);
            }
            
            return await viewRender.RenderAsync("OrganisationSearch", this);
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
    }
}