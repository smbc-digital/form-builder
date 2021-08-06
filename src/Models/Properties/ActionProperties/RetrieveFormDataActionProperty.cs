using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models.Properties.ActionProperties
{
    public partial class BaseActionProperty
    {
        public List<IncomingValue> IncomingValues { get; set; } = new List<IncomingValue>();
    }
}
