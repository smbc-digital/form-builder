
using Newtonsoft.Json;
using System.Collections.Generic;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public Source SourceObject { get; set; }
    }
    public class Source
    {
        public string Provider { get; set; }
        public List<SubmitSlug> SubmitSlugs { get; set; }
    }
}
