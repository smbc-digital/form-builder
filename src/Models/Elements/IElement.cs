using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators;
using JsonSubTypes;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace form_builder.Models.Elements
{
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    public interface IElement
    {
        [JsonConverter(typeof(StringEnumConverter))]
        EElementType Type { get; set; }

        BaseProperty Properties { get; set; }

        string Lookup { get; set; }

        bool IsValid { get; }

        void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> form_builder);

        Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null);

        Dictionary<string, dynamic> GenerateElementProperties(string type = "");

        string GenerateFieldsetProperties();

        string GetLabelText();
    }
}