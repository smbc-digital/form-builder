using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Extensions;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace form_builder.Models.Elements
{
    public class Address : Element
    {
        public const string SEARCH_QUESTION_SUFFIX = "-postcode";
        public const string SELECT_QUESTION_SUFFIX = "-address";
        public List<SelectListItem> Items { get; set; }
        public string ReturnURL { get; set; }
        public string ManualAddressURL { get; set; }
        public string AddressSearchQuestionId => $"{Properties.QuestionId}{SEARCH_QUESTION_SUFFIX}";
        public string AddressSelectQuestionId => $"{Properties.QuestionId}{SELECT_QUESTION_SUFFIX}";
        private bool IsSelect { get; set; } = false; 
        public override string  Hint => IsSelect ? Properties.SelectHint : base.Hint;
        public override string  QuestionId => IsSelect ? AddressSelectQuestionId : AddressSearchQuestionId;
        public string ChangeHeader => "Postcode";
        public override string Label
        {
            get
            {
                if(IsSelect)
                {
                    return string.IsNullOrEmpty(Properties.SelectLabel) ? "Address" : Properties.SelectLabel;
                }

                return string.IsNullOrEmpty(Properties.AddressLabel) ? "Postcode " : Properties.AddressLabel;
            }
        }
        public Address()
        {
            Type = EElementType.Address;          
        }

        public override async Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> searchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> answers, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            var isSearch =  answers.ContainsKey("AddressStatus") && answers["AddressStatus"] == "Search";
            IsSelect = answers.ContainsKey("AddressStatus") && answers["AddressStatus"] == "Select" || answers.ContainsKey(AddressSearchQuestionId) && !string.IsNullOrEmpty(answers[AddressSearchQuestionId]);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);
            Properties.Value = elementHelper.CurrentValue(this, answers, page.PageSlug, guid, SEARCH_QUESTION_SUFFIX);

            if(isSearch && !IsValid || !IsSelect)
            {
                IsSelect = false;
                return await viewRender.RenderAsync("AddressSearch", this);
            }

            Items = new List<SelectListItem>{ new SelectListItem($"{searchResults.Count} addresses found", string.Empty)};
            searchResults.ForEach((_) => { Items.Add(new SelectListItem(_.Name, $"{_.UniqueId}|{_.Name}")); });

            ReturnURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}";
            ManualAddressURL = $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}/manual";

            if (string.IsNullOrEmpty(Properties.Value))
            {
                Properties.Value = (string)answers[AddressSearchQuestionId];
                if ((string)answers["AddressStatus"] == "Select")
                {
                    Properties.Value = (string)answers[QuestionId];
                }
            }

            return await viewRender.RenderAsync("AddressSelect", this);
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
        
        public override string GetLabelText(){
            var optionalLabelText = Properties.Optional ? " (optional)" : string.Empty;
            return $"{Properties.AddressLabel}{optionalLabelText}";
        }
    }
}
