using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Configuration
{
    public class VerintConfiguration
    {
        public string EventName { get; set; }

        public int EventCode { get; set; }
    }

    public class VerintListConfiguration
    {
        public List<VerintConfiguration> FeedbackConfigurations { get; set; }
    }
}
