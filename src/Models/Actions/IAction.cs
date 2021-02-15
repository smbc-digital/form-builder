using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Providers.EmailProvider;
using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace form_builder.Models.Actions
{
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    public interface IAction
    {
        [JsonConverter(typeof(StringEnumConverter))]
        EActionType Type { get; set; }

        BaseActionProperty Properties { get; set; }
        
        Task Process(IActionHelper actionHelper, IEmailProvider emailProvider, FormAnswers formAnswers);
    }
}