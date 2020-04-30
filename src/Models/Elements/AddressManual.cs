using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class AddressManual : Element
    {
        private string[] ErrorMessages
        {
            get{
                var messages = ValidationMessage.Split(", ");
                if(messages.Length < 3)
                {
                    return new string[]{string.Empty,string.Empty, string.Empty };
                }

                return messages;
            }
        }

        public string ChangeHeader => "Postcode";

        public bool IsLine1Valid => IsValid && string.IsNullOrEmpty(Line1ValdationMessage);

        public ErrorViewModel Line1ValidationModel => new ErrorViewModel {
                Id = GetCustomItemId(AddressManualConstants.ADDRESS_LINE_1),
                IsValid = IsLine1Valid,
                Message = Line1ValdationMessage
            } ;

        public string Line1ValdationMessage => ErrorMessages[0];

        public bool IsTownValid => IsValid && string.IsNullOrEmpty(TownValdationMessage);

        public ErrorViewModel TownValidationModel => new ErrorViewModel {
        Id = GetCustomItemId(AddressManualConstants.TOWN),
                IsValid = IsTownValid,
                Message = TownValdationMessage
            } ;

        public string TownValdationMessage => ErrorMessages[1];
    
        public bool IsPostcodeValid => IsValid && string.IsNullOrEmpty(TownValdationMessage);

        public string PostcodeValdationMessage => ErrorMessages[2];

        public override string Label => Properties.AddressManualLabel;

        public ErrorViewModel PostcodeValidationModel => new ErrorViewModel {
        Id = GetCustomItemId(AddressManualConstants.POSTCODE),
                IsValid = IsPostcodeValid,
                Message = PostcodeValdationMessage
            } ;

        public string ReturnURL { get; set; }
        public AddressManual()
        {
            Type = EElementType.AddressManual;
        }

        public override string GenerateFieldsetProperties(){
            if(!string.IsNullOrWhiteSpace(Properties.AddressManualHint)){
                return $"aria-describedby = {Properties.QuestionId}-hint";
            }
            
            return string.Empty;
        }

        private Dictionary<string, dynamic> GenerateElementProperties(string errorMessage= "", string errorId = "", string autocomplete= "")
        {
            var properties = new Dictionary<string, dynamic>();
            if(!IsValid && !string.IsNullOrEmpty(errorMessage))
            {
                properties.Add("aria-describedby", errorId);
            }

            if(!string.IsNullOrEmpty(autocomplete))
            {
                properties.Add("autocomplete", autocomplete);
            }
            
            return properties;
        }

        public Dictionary<string, dynamic> GenerateAddress1ElementProperties() => GenerateElementProperties(Line1ValdationMessage, GetCustomErrorId(AddressManualConstants.ADDRESS_LINE_1), "address-line1");

        public Dictionary<string, dynamic> GenerateAddress2ElementProperties() => GenerateElementProperties(autocomplete: "address-line2");

        public Dictionary<string, dynamic> GenerateTownElementProperties() => GenerateElementProperties(TownValdationMessage, GetCustomErrorId(AddressManualConstants.TOWN), "address-level1");

        public Dictionary<string, dynamic> GeneratePostcodeElementProperties() => GenerateElementProperties(PostcodeValdationMessage, GetCustomErrorId(AddressManualConstants.POSTCODE), "postal-code");
        
        protected void SetAddressProperties(Dictionary<string, dynamic> viewModel, string searchTerm)
        {
            Properties.AddressManualAddressLine1 = viewModel.FirstOrDefault(_ => _.Key.Contains(AddressManualConstants.ADDRESS_LINE_1)).Value;
            Properties.AddressManualAddressLine2 = viewModel.FirstOrDefault(_ => _.Key.Contains(AddressManualConstants.ADDRESS_LINE_2)).Value;
            Properties.AddressManualAddressTown = viewModel.FirstOrDefault(_ => _.Key.Contains(AddressManualConstants.TOWN)).Value;
            Properties.AddressManualAddressPostcode = viewModel.FirstOrDefault(_ => _.Key.Contains(AddressManualConstants.POSTCODE)).Value ?? searchTerm;           
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            if (viewModel.ContainsKey("AddressStatus"))
            {
                viewModel.Remove("AddressStatus");
            };

            viewModel.Add("AddressStatus", "Manual");
            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-postcode");
            SetAddressProperties(viewModel, Properties.Value);
            var searchResultsCount =  elementHelper.GetFormDataValue(guid, $"{Properties.QuestionId}-srcount");
            var isValid = int.TryParse(searchResultsCount.ToString(), out int output);

            if (isValid && output == 0)
            {
                Properties.DisplayNoResultsIAG = true;
            }
            
            ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
            return await viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}
