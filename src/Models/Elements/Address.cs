using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;

namespace form_builder.Models.Elements
{
    public class Address : Element
    {
        public Address() => Type = EElementType.Address;

        public List<SelectListItem> Items { get; set; }

        public string ReturnURL { get; set; }

        public string ManualAddressURL { get; set; }

        public string AddressSearchQuestionId => $"{Properties.QuestionId}{AddressConstants.SEARCH_SUFFIX}";

        public string AddressSelectQuestionId => $"{Properties.QuestionId}{AddressConstants.SELECT_SUFFIX}";

        private bool IsSelect { get; set; } = false;

        public override string Hint => IsSelect ? Properties.SelectHint : base.Hint;

        public override bool DisplayHint => !string.IsNullOrEmpty(Hint);

        public override string QuestionId => IsSelect ? AddressSelectQuestionId : AddressSearchQuestionId;

        public string ChangeHeader => "Postcode:";

        public override string Label
        {
            get
            {
                if (IsSelect)
                    return string.IsNullOrEmpty(Properties.SelectLabel) ? "Select the address from the list" : Properties.SelectLabel;

                return string.IsNullOrEmpty(Properties.AddressLabel) ? "Postcode" : Properties.AddressLabel;
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
            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);
            switch (subPath as string)
            {
                case LookUpConstants.Manual:
                    var manualAddressElement = new AddressManual(validationResult)
                    {
                        Properties = Properties
                    };
                    return await manualAddressElement.RenderAsync(viewRender, elementHelper, cacheKey, viewModel, page, formSchema, environment, formAnswers, results);

                case LookUpConstants.Automatic:
                    if (results is null)
                        throw new ApplicationException("Address::RenderAsync: retrieved automatic address search results cannot be null");

                    IsSelect = true;
                    Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, AddressConstants.SEARCH_SUFFIX);

                    ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

                    ManualAddressURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}/manual";

                    var selectedAddress = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, AddressConstants.SELECT_SUFFIX);
                    var searchSuffix = results.Count.Equals(1) ? "address found" : "addresses found";
                    Items = new List<SelectListItem> { new SelectListItem($"{results.Count} {searchSuffix}", string.Empty) };

                    results.ForEach((objectResult) =>
                    {
                        AddressSearchResult searchResult;

                        if ((objectResult as JObject) is not null)
                            searchResult = (objectResult as JObject).ToObject<AddressSearchResult>();
                        else
                            searchResult = objectResult as AddressSearchResult;

                        Items.Add(new SelectListItem(
                            searchResult.Name,
                            $"{searchResult.UniqueId}|{searchResult.Name}", searchResult.UniqueId.Equals(selectedAddress)));
                    });

                    return await viewRender.RenderAsync("AddressSelect", this);

                default:
                    Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, AddressConstants.SEARCH_SUFFIX);
                    return await viewRender.RenderAsync("AddressSearch", this);
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

        public override string GenerateFieldsetProperties()
        {
            return !string.IsNullOrWhiteSpace(Properties.AddressManualHint)
                ? $"aria-describedby = {Properties.QuestionId}-hint"
                : string.Empty;
        }
    }
}