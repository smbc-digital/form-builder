using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Validators;
using JsonSubTypes;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    [JsonConverter(typeof(JsonSubtypes), "Type")]   
    public interface IElement
    {
            EElementType Type { get; set; }

            Property Properties { get; set; }

            bool IsValid { get; }

            void Validate(Dictionary<string, string> viewModel, IEnumerable<IElementValidator> form_builder);

            Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, Dictionary<string, string> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment);
    }
}
