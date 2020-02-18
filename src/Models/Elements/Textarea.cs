using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class Textarea : Element
    {
        public Textarea()
        {
            Type = EElementType.Textarea;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckForMaxLength(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }

        public override Dictionary<string, dynamic> GenerateElementProperties()
        {
            var properties = new Dictionary<string, dynamic>()
            {
                { "name", Properties.QuestionId },
                { "id", Properties.QuestionId },
                { "maxlength", Properties.MaxLength },
                { "value", Properties.Value},
                { "autocomplete", "on" }
            };

            if (Properties.MaxLength <= 200 || Properties.MaxLength > 500)
            {
                properties.Add("class", Properties.MaxLength > 500 ? "large" : "small");
            }

            if (DisplayAriaDescribedby)
            {
                properties.Add("aria-describedby", DescribedByValue());
            }

            return properties;
        }
    }
}
