using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Configuration
{
    public class ProviderAvailabilityConfiguration
    {
        public string[] ElementTypes { get; set; }
        public string[] ProviderNames { get; set; }
        public string[] Environments { get; set; }
        public List<EnabledForBase> DisabledFor { get; set; }
    }
}
