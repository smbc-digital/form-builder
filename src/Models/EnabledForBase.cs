using System.Text.Json.Serialization;
using form_builder.Enum;
using form_builder.Models.Properties.EnabledForProperties;
using Newtonsoft.Json.Converters;

namespace form_builder.Models
{
    public class EnabledForBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EEnabledFor Type { get; set; }
        public EnabledForProperties Properties { get; set; }
    }
}