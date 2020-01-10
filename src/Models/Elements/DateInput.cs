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
    public class DateInput : Element
    {
        public DateInput()
        {
            Type = EElementType.DateInput;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            Properties.Day = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-day");
            Properties.Month = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-month");
            Properties.Year = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, "-year");
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}
