using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using StockportGovUK.NetStandard.Gateways.Models.Verint.Lookup;

namespace form_builder.Models.Elements;

public class Organisation : Element
{
    public List<SelectListItem> Items { get; set; }

    public string ReturnURL { get; set; }

    public string OrganisationSearchQuestionId => $"{Properties.QuestionId}";

    public string OrganisationSelectQuestionId => $"{Properties.QuestionId}{OrganisationConstants.SELECT_SUFFIX}";

    private bool IsSelect { get; set; } = false;

    public override string Hint => IsSelect ? Properties.SelectHint : base.Hint;

    public override bool DisplayHint => !string.IsNullOrEmpty(Hint);

    public override string QuestionId => IsSelect ? OrganisationSelectQuestionId : OrganisationSearchQuestionId;

    public string ChangeHeader => "Organisation:";

    public override string Label
    {
        get
        {
            if (IsSelect)
                return string.IsNullOrEmpty(Properties.SelectLabel) ? "Select the organisation below" : Properties.SelectLabel;

            return string.IsNullOrEmpty(Properties.Label) ? "Organisation name" : Properties.Label;
        }
    }

    public override string GetLabelText(string pageTitle) => $"{(string.IsNullOrEmpty(Properties.SummaryLabel) ? pageTitle : Properties.SummaryLabel)}{GetIsOptionalLabelText()}";

    public Organisation() => Type = EElementType.Organisation;

    public override async Task<string> RenderAsync(IViewRender viewRender,
        IElementHelper elementHelper,
        string cacheKey,
        Dictionary<string, dynamic> viewModel,
        Page page,
        FormSchema formSchema,
        IWebHostEnvironment environment,
        FormAnswers formAnswers,
        List<object> results = null)
    {
        elementHelper.CheckForQuestionId(this);
        elementHelper.CheckForProvider(this);
        viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

        switch (subPath as string)
        {
            case LookUpConstants.Automatic:
                Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers);
                IsSelect = true;
                ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

                var selectedOrganisation = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, OrganisationConstants.SELECT_SUFFIX);
                Items = new List<SelectListItem> { new SelectListItem($"{results?.Count} organisations found", string.Empty) };

                results?.ForEach(objectResult =>
                {
                    OrganisationSearchResult searchResult;

                    if (objectResult as JObject is not null)
                        searchResult = (objectResult as JObject).ToObject<OrganisationSearchResult>();
                    else
                        searchResult = objectResult as OrganisationSearchResult;

                    Items.Add(new SelectListItem(searchResult.Name, $"{searchResult.Reference}|{searchResult.Name}", searchResult.Reference.Equals(selectedOrganisation)));
                });

                return await viewRender.RenderAsync("OrganisationSelect", this);

            default:
                Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers);
                var test = await viewRender.RenderAsync("OrganisationSearch", this);

                return test;
        }
    }

    public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
    {
        var elementProperties = new Dictionary<string, dynamic>
        {
            {"id", $"{QuestionId}"},
            {"name", $"{QuestionId}"}
        };

        if (DisplayAriaDescribedby)
            elementProperties.Add("aria-describedby", GetDescribedByAttributeValue());

        if (string.IsNullOrEmpty(type))
            elementProperties.Add("maxlength", Properties.MaxLength);

        return elementProperties;
    }
}