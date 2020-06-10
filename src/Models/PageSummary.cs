using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models
{
    public class PageSummary
    {
        public string  PageTitle {get;set;}
        public string PageSlug { get; set; }

        public Dictionary<string, string> Answers { get; set; }
    }
}
