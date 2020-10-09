using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Validators;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Models.Elements
{
    public class AddressManual : Element
    {
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

        public ErrorViewModel Line1ValidationModel => new ErrorViewModel
        {
            Id = GetCustomErrorId(AddressManualConstants.ADDRESS_LINE_1),
            IsValid = IsLine1Valid,
            Message = Line1ValidationMessage
        };

        public string Line1ValidationMessage => ErrorMessages[0];

        public bool IsTownValid => string.IsNullOrEmpty(TownValidationMessage);

        public ErrorViewModel TownValidationModel => new ErrorViewModel
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
        public AddressManual()
        {
            Type = EElementType.AddressManual;
        }

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
            => GenerateElementProperties(Line1ValidationMessage, GetCustomErrorId(AddressManualConstants.ADDRESS_LINE_1), "address-line1");

        public Dictionary<string, dynamic> GenerateAddress2ElementProperties()
            => GenerateElementProperties(autocomplete: "address-line2");

        public Dictionary<string, dynamic> GenerateTownElementProperties()
            => GenerateElementProperties(TownValidationMessage, GetCustomErrorId(AddressManualConstants.TOWN), "address-level1");

        public Dictionary<string, dynamic> GeneratePostcodeElementProperties()
            => GenerateElementProperties(PostcodeValidationMessage, GetCustomErrorId(AddressManualConstants.POSTCODE), "postal-code");

        protected void SetAddressProperties(IElementHelper elementHelper, FormAnswers formAnswers, string pageSlug, string guid, Dictionary<string, dynamic> viewModel)
        {
            Properties.Value = elementHelper.CurrentValue(this, viewModel, formAnswers, pageSlug, guid, AddressConstants.SEARCH_SUFFIX);
            Properties.AddressManualAddressLine1 = elementHelper.CurrentValue(this, viewModel, formAnswers, pageSlug, guid, $"-{AddressManualConstants.ADDRESS_LINE_1}");
            Properties.AddressManualAddressLine2 = elementHelper.CurrentValue(this, viewModel, formAnswers, pageSlug, guid, $"-{AddressManualConstants.ADDRESS_LINE_2}");
            Properties.AddressManualAddressTown = elementHelper.CurrentValue(this, viewModel, formAnswers, pageSlug, guid, $"-{AddressManualConstants.TOWN}");
            Properties.AddressManualAddressPostcode = viewModel.FirstOrDefault(_ => _.Key.Contains(AddressManualConstants.POSTCODE)).Value;

            if (string.IsNullOrEmpty(Properties.AddressManualAddressPostcode))
            {
                var value = elementHelper.CurrentValue(this, viewModel, formAnswers, pageSlug, guid, $"-{AddressManualConstants.POSTCODE}");
                Properties.AddressManualAddressPostcode = string.IsNullOrEmpty(value) ? Properties.Value : value;
            }   
        }

        public override async Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            SetAddressProperties(elementHelper, formAnswers, page.PageSlug, guid, viewModel);

            if (results != null && results.Count == 0)
                Properties.DisplayNoResultsIAG = true;

            ReturnURL = environment.EnvironmentName.Equals("local") || environment.EnvironmentName.Equals("uitest")
                ? $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}"
                : $"{environment.EnvironmentName.ToReturnUrlPrefix()}/v2/{formSchema.BaseURL}/{page.PageSlug}";

            return await viewRender.RenderAsync("AddressManual", this);
        }
    }
}