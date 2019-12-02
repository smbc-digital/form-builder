using form_builder.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace form_builder.Models
{
    public class Behaviour
    {
        public List<Condition> Conditions { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EBehaviourType BehaviourType { get; set; }

        public string PageSlug { get; set; }
    }
}