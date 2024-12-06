using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Validators;
using form_builder.ViewModels;

namespace form_builder.Models.Elements;

public class AddressManual : Element
{
    public AddressManual() => Type = EElementType.AddressManual;

    private string[] ErrorMessages
    {
        get
        {
            var messages = ValidationMessage.Split(", ");

            return messages.Length < 3 ? new string[] { string.Empty, string.Empty, string.Empty } : messages;
        }
    }

    public string ChangeHeader => "Postcode:";

    public bool IsLine1Valid => string.IsNullOrEmpty(Line1ValidationMessage);

    public ErrorViewModel Line1ValidationModel => new()
    {
        Id = GetCustomErrorId(AddressManualConstants.ADDRESS_LINE_1),
        IsValid = IsLine1Valid,
        Message = Line1ValidationMessage
    };

    public string Line1ValidationMessage => ErrorMessages[0];

    public bool IsTownValid => string.IsNullOrEmpty(TownValidationMessage);

    public ErrorViewModel TownValidationModel => new()
    {
        Id = GetCustomErrorId(AddressManualConstants.TOWN),
        IsValid = IsTownValid,
        Message = TownValidationMessage
    };

    public string TownValidationMessage => ErrorMessages[1];

    public bool IsPostcodeValid => string.IsNullOrEmpty(PostcodeValidationMessage);

    public string PostcodeValidationMessage => ErrorMessages[2];

    public override string Label => Properties.AddressManualLabel;

    public ErrorViewModel PostcodeValidationModel => new ErrorViewModel
    {
        Id = GetCustomErrorId(AddressManualConstants.POSTCODE),
        IsValid = IsPostcodeValid,
        Message = PostcodeValidationMessage
    };

    public string ReturnURL { get; set; }

    public AddressManual(ValidationResult validation)
    {
        Type = EElementType.AddressManual;
        validationResult = validation;
    }

    public override string GenerateFieldsetProperties() =>
        !string.IsNullOrWhiteSpace(Properties.AddressManualHint)
            ? $"aria-describedby = {Properties.QuestionId}-hint"
            : string.Empty;

    private Dictionary<string, dynamic> GenerateElementProperties(string errorMessage = "", string errorId = "", string autocomplete = "")
    {
        var properties = new Dictionary<string, dynamic>();
        if (!IsValid && !string.IsNullOrEmpty(errorMessage))
            properties.Add("aria-describedby", errorId);

        if (!string.IsNullOrEmpty(autocomplete))
            properties.Add("autocomplete", autocomplete);

        return properties;
    }

    public Dictionary<string, dynamic> GenerateAddress1ElementProperties()
    {
        var properties = GenerateElementProperties(Line1ValidationMessage, GetCustomErrorId(AddressManualConstants.ADDRESS_LINE_1), "address-line1");
        properties.Add("maxlength", Properties.AddressManualLineMaxLength);

        return properties;
    }

    public Dictionary<string, dynamic> GenerateAddress2ElementProperties()
    {
        var properties = GenerateElementProperties(autocomplete: "address-line2");
        properties.Add("maxlength", Properties.AddressManualLineMaxLength);

        return properties;
    }

    public Dictionary<string, dynamic> GenerateTownElementProperties()
    {
        var properties = GenerateElementProperties(TownValidationMessage, GetCustomErrorId(AddressManualConstants.TOWN), "address-level1");
        properties.Add("maxlength", Properties.AddressManualLineMaxLength);

        return properties;
    }

    public Dictionary<string, dynamic> GeneratePostcodeElementProperties()
    {
        var properties = GenerateElementProperties(PostcodeValidationMessage, GetCustomErrorId(AddressManualConstants.POSTCODE), "postal-code");
        properties.Add("maxlength", Properties.AddressManualPostcodeMaxLength);

        return properties;
    }

    protected void SetAddressProperties(IElementHelper elementHelper, FormAnswers formAnswers, string pageSlug, string cacheKey, Dictionary<string, dynamic> viewModel)
    {
        Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, AddressConstants.SEARCH_SUFFIX);
        Properties.AddressManualAddressLine1 = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, $"-{AddressManualConstants.ADDRESS_LINE_1}");
        Properties.AddressManualAddressLine2 = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, $"-{AddressManualConstants.ADDRESS_LINE_2}");
        Properties.AddressManualAddressTown = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, $"-{AddressManualConstants.TOWN}");
        Properties.AddressManualAddressPostcode = viewModel.FirstOrDefault(_ => _.Key.Contains(AddressManualConstants.POSTCODE)).Value;

        if (string.IsNullOrEmpty(Properties.AddressManualAddressPostcode))
        {
            var value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, $"-{AddressManualConstants.POSTCODE}");
            Properties.AddressManualAddressPostcode = string.IsNullOrEmpty(value) ? Properties.Value : value;
        }
    }

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
        SetAddressProperties(elementHelper, formAnswers, page.PageSlug, cacheKey, viewModel);

        if (results is not null && results.Count.Equals(0))
            Properties.DisplayNoResultsIAG = true;

        ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";

        return await viewRender.RenderAsync("AddressManual", this);
    }
}