using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;

namespace form_builder.Models.Elements;

public class Street : Element
{
    public Street() => Type = EElementType.Street;

    public List<SelectListItem> Items { get; set; }

    public string ReturnURL { get; set; }

    public string StreetSearchQuestionId => $"{Properties.QuestionId}";

    public string StreetSelectQuestionId => $"{Properties.QuestionId}{StreetConstants.SELECT_SUFFIX}";

    private bool IsSelect { get; set; } = false;

    public override string Hint => IsSelect ? Properties.SelectHint : base.Hint;

    public override bool DisplayHint => !string.IsNullOrEmpty(Hint);

    public override string QuestionId => IsSelect ? StreetSelectQuestionId : StreetSearchQuestionId;

    public string ChangeHeader => "Street:";

    public override string Label
    {
        get
        {
            if (IsSelect)
                return string.IsNullOrEmpty(Properties.SelectLabel) ? "Select the street from the list" : Properties.SelectLabel;

            return string.IsNullOrEmpty(Properties.Label) ? "Street name" : Properties.Label;
        }
    }

    public override string GetLabelText(string pageTitle) => $"{(string.IsNullOrEmpty(Properties.SummaryLabel) ? pageTitle : Properties.SummaryLabel)}{GetIsOptionalLabelText()}";

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
                IsSelect = true;
                Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers);

                ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

                var selectedStreet = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, StreetConstants.SELECT_SUFFIX);
                var searchSuffix = results?.Count is 1 ? "street found" : "streets found";
                Items = new List<SelectListItem> { new SelectListItem($"{results?.Count} {searchSuffix}", string.Empty) };

                results?.ForEach((objectResult) =>
                {
                    AddressSearchResult searchResult;

                    if (objectResult as JObject is not null)
                        searchResult = (objectResult as JObject).ToObject<AddressSearchResult>();
                    else
                        searchResult = objectResult as AddressSearchResult;

                    Items.Add(new SelectListItem(searchResult.Name, $"{searchResult.UniqueId}|{searchResult.Name}", searchResult.UniqueId.Equals(selectedStreet)));
                });

                return await viewRender.RenderAsync("StreetSelect", this);

            default:

                Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers);
                return await viewRender.RenderAsync("StreetSearch", this);
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