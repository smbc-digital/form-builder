using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Validators;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class AddressManual : Element
    {
        private string[] ErrorMessages
        {
            get
            {
                var messages = ValidationMessage.Split(", ");
                if (messages.Length < 3)
                {
                    return new string[] { string.Empty, string.Empty, string.Empty };
                }

                return messages;
            }
        }

        public string ChangeHeader => "Postcode:";

        public bool IsLine1Valid => string.IsNullOrEmpty(Line1ValdationMessage);

        public ErrorViewModel Line1ValidationModel => new ErrorViewModel
        {
            Id = GetCustomErrorId(AddressManualConstants.ADDRESS_LINE_1),
            IsValid = IsLine1Valid,
            Message = Line1ValdationMessage
        };

        public string Line1ValdationMessage => ErrorMessages[0];

        public bool IsTownValid => string.IsNullOrEmpty(TownValdationMessage);

        public ErrorViewModel TownValidationModel => new ErrorViewModel
        {
            Id = GetCustomErrorId(AddressManualConstants.TOWN),
            IsValid = IsTownValid,
            Message = TownValdationMessage
        };

        public string TownValdationMessage => ErrorMessages[1];

        public bool IsPostcodeValid => string.IsNullOrEmpty(PostcodeValdationMessage);

        public string PostcodeValdationMessage => ErrorMessages[2];

        public override string Label => Properties.AddressManualLabel;

        public ErrorViewModel PostcodeValidationModel => new ErrorViewModel
        {
            Id = GetCustomErrorId(AddressManualConstants.POSTCODE),
            IsValid = IsPostcodeValid,
            Message = PostcodeValdationMessage
        };

        public string ReturnURL { get; set; }
        public AddressManual()
        {
            Type = EElementType.AddressManual;
        }

        public AddressManual(ValidationResult validaiton)
        {
            Type = EElementType.AddressManual;
            validationResult = validaiton;
        }

        public override string GenerateFieldsetProperties()
        {
            if (!string.IsNullOrWhiteSpace(Properties.AddressManualHint))
            {
                return $"aria-describedby = {Properties.QuestionId}-hint";
            }

            return string.Empty;
        }

        private Dictionary<string, dynamic> GenerateElementProperties(string errorMessage = "", string errorId = "", string autocomplete = "")
        {
            var properties = new Dictionary<string, dynamic>();
            if (!IsValid && !string.IsNullOrEmpty(errorMessage))
            {
                properties.Add("aria-describedby", errorId);
            }

            if (!string.IsNullOrEmpty(autocomplete))
            {
                properties.Add("autocomplete", autocomplete);
            }

            return properties;
        }

        public Dictionary<string, dynamic> GenerateAddress1ElementProperties() => GenerateElementProperties(Line1ValdationMessage, GetCustomErrorId(AddressManualConstants.ADDRESS_LINE_1), "address-line1");

        public Dictionary<string, dynamic> GenerateAddress2ElementProperties() => GenerateElementProperties(autocomplete: "address-line2");

        public Dictionary<string, dynamic> GenerateTownElementProperties() => GenerateElementProperties(TownValdationMessage, GetCustomErrorId(AddressManualConstants.TOWN), "address-level1");

        public Dictionary<string, dynamic> GeneratePostcodeElementProperties() => GenerateElementProperties(PostcodeValdationMessage, GetCustomErrorId(AddressManualConstants.POSTCODE), "postal-code");

        protected void SetAddressProperties(IElementHelper elementHelper, string pageSlug, string guid, Dictionary<string, dynamic> viewModel)
        {
            Properties.Value = elementHelper.CurrentValue(this, viewModel, pageSlug, guid, AddressConstants.SEARCH_SUFFIX);
            Properties.AddressManualAddressLine1 = elementHelper.CurrentValue(this, viewModel, pageSlug, guid, $"-{AddressManualConstants.ADDRESS_LINE_1}");
            Properties.AddressManualAddressLine2 = elementHelper.CurrentValue(this, viewModel, pageSlug, guid, $"-{AddressManualConstants.ADDRESS_LINE_2}");
            Properties.AddressManualAddressTown = elementHelper.CurrentValue(this, viewModel, pageSlug, guid, $"-{AddressManualConstants.TOWN}");
            Properties.AddressManualAddressPostcode = viewModel.FirstOrDefault(_ => _.Key.Contains(AddressManualConstants.POSTCODE)).Value;

            if(string.IsNullOrEmpty(Properties.AddressManualAddressPostcode))
            {
                var value = elementHelper.CurrentValue(this, viewModel, pageSlug, guid, $"-{AddressManualConstants.POSTCODE}");
                Properties.AddressManualAddressPostcode = string.IsNullOrEmpty(value) ? Properties.Value : value;
            }   
        }

        public override async Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IHostingEnvironment environment,
            List<object> results = null)
        {
            SetAddressProperties(elementHelper, page.PageSlug, guid, viewModel);

            if (results != null && results.Count == 0)
                Properties.DisplayNoResultsIAG = true;

            ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
            return await viewRender.RenderAsync("AddressManual", this);
        }
    }
}
