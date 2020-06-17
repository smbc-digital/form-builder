using form_builder.Constants;
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
        public List<SelectListItem> Items { get; set; }
        public string ReturnURL { get; set; }
        public string OrganisationSearchQuestionId => $"{Properties.QuestionId}";
        public string OrganisationSelectQuestionId => $"{Properties.QuestionId}{OrganisationConstants.SELECT_SUFFIX}";
        private bool IsSelect { get; set; } = false; 
        public override string  Hint => IsSelect ? Properties.SelectHint : base.Hint;
        public override bool DisplayHint => !string.IsNullOrEmpty(Hint);
        public override string  QuestionId => IsSelect ? OrganisationSelectQuestionId : OrganisationSearchQuestionId;
        public string ChangeHeader => "Organisation:";
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
            Properties.Value = elementHelper.CurrentValue(this, answers, page.PageSlug, guid);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);

            if (!IsSelect)
            {
                return await viewRender.RenderAsync("OrganisationSearch", this);
            }

            Items = new List<SelectListItem>{ new SelectListItem($"{organisationResults.Count} organisations found", string.Empty)};
            organisationResults.ForEach((_) => { Items.Add(new SelectListItem(_.Name, $"{_.Reference}|{_.Name}")); });
            ReturnURL = environment.EnvironmentName == "local" || environment.EnvironmentName == "uitest"
                ? $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}"
                : $"{environment.EnvironmentName.ToReturnUrlPrefix()}/v2/{formSchema.BaseURL}/{page.PageSlug}";
            return await viewRender.RenderAsync("OrganisationSelect", this);
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