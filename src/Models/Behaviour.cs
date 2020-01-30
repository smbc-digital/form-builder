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

        public List<PageSlug> PageSlugs { get; set; }
    }

    public class PageSlug
    {
        public string Location { get; set; }
        public string URL { get; set; }
    }
}