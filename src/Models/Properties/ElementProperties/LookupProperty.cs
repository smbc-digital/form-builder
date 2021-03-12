using System.Collections.Generic;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public List<Lookup> Lookup { get; set; }
    }
    public class Lookup
    {
        public string EnvironmentName { get; set; }

        public string Provider { get; set; }

        public string URL { get; set; }

        public string AuthToken { get; set; }
    }
}
