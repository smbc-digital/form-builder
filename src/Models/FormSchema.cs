using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Models
{
    public class FormSchema
    {
        public string FormName { get; set; }
        public string BaseURL { get; set; }
        public string StartPageSlug { get; set; }
        public string FeedbackForm { get; set; }
        public List<Page> Pages { get; set; }

        public Page GetPage(string path)
        {
            var page = Pages.FirstOrDefault(_ => _.PageSlug.ToLower() == path.ToLower());

            return page;
        }
    }
}