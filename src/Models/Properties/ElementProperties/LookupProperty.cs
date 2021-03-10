
using System.Collections.Generic;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public Lookup Lookup { get; set; }
    }
    public class Lookup
    {
        public string Provider { get; set; }
        public List<SubmitSlug> Environments { get; set; }
    }
}
