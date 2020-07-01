using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

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
                    return string.IsNullOrEmpty(Properties.SelectLabel) ? "Select the organisation below" : Properties.SelectLabel;

                return string.IsNullOrEmpty(Properties.Label) ? "Organisation name" : Properties.Label;
            }
        }

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
                    Properties.Value = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, string.Empty);
                    IsSelect = true;
                    ReturnURL = environment.EnvironmentName == "local" || environment.EnvironmentName == "uitest"
                                ? $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}"
                                : $"{environment.EnvironmentName.ToReturnUrlPrefix()}/v2/{formSchema.BaseURL}/{page.PageSlug}";

                    var selectedOrganisation = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, OrganisationConstants.SELECT_SUFFIX);
                    Items = new List<SelectListItem> { new SelectListItem($"{results?.Count} organisations found", string.Empty) };
                    results?.ForEach((objectResult) =>
                    {
                        OrganisationSearchResult searchResult;

                        if ((objectResult as JObject) != null)
                            searchResult = (objectResult as JObject).ToObject<OrganisationSearchResult>();
                        else
                            searchResult = objectResult as OrganisationSearchResult;

                        Items.Add(new SelectListItem(searchResult.Name, $"{searchResult.Reference}|{searchResult.Name}", searchResult.Reference.Equals(selectedOrganisation)));
                    });

                    return await viewRender.RenderAsync("OrganisationSelect", this);
                default:
                    Properties.Value = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, string.Empty);
                    var test = await viewRender.RenderAsync("OrganisationSearch", this);
                    return test;
            }
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type="")
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