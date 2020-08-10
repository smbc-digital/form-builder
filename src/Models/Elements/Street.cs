using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Models.Elements
{
    public class Street : Element
    {
        public List<SelectListItem> Items { get; set; }

        public string ReturnURL { get; set; }

        public string StreetSearchQuestionId => $"{Properties.QuestionId}";

        public string StreetSelectQuestionId => $"{Properties.QuestionId}{StreetConstants.SELECT_SUFFIX}";

        private bool IsSelect { get; set; } = false; 

        public override string  Hint => IsSelect ? Properties.SelectHint : base.Hint;

        public override bool DisplayHint => !string.IsNullOrEmpty(Hint);

        public override string  QuestionId => IsSelect ? StreetSelectQuestionId : StreetSearchQuestionId;

        public string ChangeHeader => "Street:";
        
        public override string Label
        {
            get
            {
                if (IsSelect)
                    return string.IsNullOrEmpty(Properties.SelectLabel) ? "Select the street below" : Properties.SelectLabel;

                return string.IsNullOrEmpty(Properties.Label) ? "Street name" : Properties.Label;
            }
        }

        public Street()
        {
            Type = EElementType.Street;
        }

        public override async Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> answers,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            List<object> results = null)
        {
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForProvider(this);
            answers.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

            switch (subPath as string)
            {
                case LookUpConstants.Automatic:
                    IsSelect = true;
                    Properties.Value = elementHelper.CurrentValue<string>(this, answers, page.PageSlug, guid, string.Empty);

                    ReturnURL = environment.EnvironmentName.Equals("local") || environment.EnvironmentName.Equals("uitest")
                        ? $"{environment.EnvironmentName.ToReturnUrlPrefix()}/{formSchema.BaseURL}/{page.PageSlug}"
                        : $"{environment.EnvironmentName.ToReturnUrlPrefix()}/v2/{formSchema.BaseURL}/{page.PageSlug}";

                    var selectedStreet = elementHelper.CurrentValue<string>(this, answers, page.PageSlug, guid, StreetConstants.SELECT_SUFFIX);
                    Items = new List<SelectListItem> { new SelectListItem($"{results?.Count} streets found", string.Empty) };
                    results?.ForEach((objectResult) => {
                        AddressSearchResult searchResult;

                        if (objectResult as JObject != null)
                            searchResult = (objectResult as JObject).ToObject<AddressSearchResult>();
                        else
                            searchResult = objectResult as AddressSearchResult;

                        Items.Add(new SelectListItem(searchResult.Name, $"{searchResult.UniqueId}|{searchResult.Name}", searchResult.UniqueId.Equals(selectedStreet)));
                    });

                    return await viewRender.RenderAsync("StreetSelect", this);

                default:

                    Properties.Value = elementHelper.CurrentValue<string>(this, answers, page.PageSlug, guid, string.Empty);
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

            if(string.IsNullOrEmpty(type))
                elementProperties.Add("maxlength", Properties.MaxLength);
 
            return elementProperties;
        }
    }
}