using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using form_builder.Constants;
using Newtonsoft.Json.Linq;
using System.Linq;
using form_builder.Enum;

namespace form_builder.Models.Elements
{
    public class Address : Element
    {
        public List<SelectListItem> Items { get; set; }
        public string ReturnURL { get; set; }
        public string ManualAddressURL { get; set; }
        public string AddressSearchQuestionId => $"{Properties.QuestionId}{AddressConstants.SEARCH_SUFFIX}";
        public string AddressSelectQuestionId => $"{Properties.QuestionId}{AddressConstants.SELECT_SUFFIX}";
        private bool IsSelect { get; set; } = false; 
        private bool IsSearch { get; set; } = false; 
        public override string  Hint => IsSelect ? Properties.SelectHint : base.Hint;
        public override bool DisplayHint => !string.IsNullOrEmpty(Hint);
        public override string  QuestionId => IsSelect ? AddressSelectQuestionId : AddressSearchQuestionId;
        public string ChangeHeader => "Postcode:";
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

        public override async Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IHostingEnvironment environment,
            List<object> results = null)
        {
            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);
            switch (subPath as string) {
                case LookUpConstants.Manual:
                    var manualAddressElement = new AddressManual(validationResult);
                    manualAddressElement.Properties = Properties;

                    return await manualAddressElement.RenderAsync(viewRender,elementHelper, guid, viewModel, page,formSchema, environment, results);
                case LookUpConstants.Automatic:
                    IsSelect = true;
                    Properties.Value = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, AddressConstants.SEARCH_SUFFIX);

                    ReturnURL = environment.EnvironmentName == "local" || environment.EnvironmentName == "uitest" 
                        ? $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}" 
                        : $"{environment.EnvironmentName.ToReturnUrlPrefix()}/v2/{formSchema.BaseURL}/{page.PageSlug}";
                    
                    ManualAddressURL = environment.EnvironmentName == "local" || environment.EnvironmentName == "uitest"
                        ? $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}/manual"
                        : $"{environment.EnvironmentName.ToReturnUrlPrefix()}/v2/{formSchema.BaseURL}/{page.PageSlug}/manual";

                    var selectedAddress = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, AddressConstants.SELECT_SUFFIX);
                    Items = new List<SelectListItem> { new SelectListItem($"{results.Count} addresses found", string.Empty) };
                    results.ForEach((objectResult) => {
                        AddressSearchResult searchResult;

                        if ((objectResult as JObject) != null)
                        {
                            searchResult = (objectResult as JObject).ToObject<AddressSearchResult>();
                        }
                        else
                        {
                            searchResult = objectResult as AddressSearchResult;
                        }

                        Items.Add(new SelectListItem(
                            searchResult.Name,
                            $"{searchResult.UniqueId}|{searchResult.Name}", searchResult.UniqueId.Equals(selectedAddress)));
                    });

                    return await viewRender.RenderAsync("AddressSelect", this);

                default:
                    Properties.Value = elementHelper.CurrentValue<string>(this, viewModel, page.PageSlug, guid, AddressConstants.SEARCH_SUFFIX);
                    return await viewRender.RenderAsync("AddressSearch", this);
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
        
        public override string GetLabelText(){
            var optionalLabelText = Properties.Optional ? " (optional)" : string.Empty;
            return $"{Properties.AddressLabel}{optionalLabelText}";
        }

        private void SetAddressProperties(Dictionary<string, dynamic> viewModel, string searchTerm)
        {
            Properties.AddressManualAddressLine1 = viewModel.FirstOrDefault(_ => _.Key.Contains("AddressManualAddressLine1")).Value;
            Properties.AddressManualAddressLine2 = viewModel.FirstOrDefault(_ => _.Key.Contains("AddressManualAddressLine2")).Value;
            Properties.AddressManualAddressTown = viewModel.FirstOrDefault(_ => _.Key.Contains("AddressManualAddressTown")).Value;
            Properties.AddressManualAddressPostcode = viewModel.FirstOrDefault(_ => _.Key.Contains("AddressManualAddressPostcode")).Value ?? searchTerm;
        }

        public override string GenerateFieldsetProperties()
        {
            if (!string.IsNullOrWhiteSpace(Properties.AddressManualHint))
            {
                return $"aria-describedby = {Properties.QuestionId}-hint";
            }

            return string.Empty;
        }
    }
}
