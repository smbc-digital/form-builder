using System.Collections.Generic;

namespace form_builder.Models
{
    public class PageSummary
    {
        public string  PageTitle {get;set;}

        public string PageSlug { get; set; }

        public Dictionary<string, string> Answers { get; set; }
    }
}