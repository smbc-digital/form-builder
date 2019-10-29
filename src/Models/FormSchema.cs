using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Models
{
    public class FormSchema
    {
        public string Name { get; set; }
        public string BaseURL { get; set; }
        public string StartPage { get; set; }
        public string FeedbackForm { get; set; }
        public List<Page> Pages { get; set; }

        public Page GetPage(string path)
        {
            var page = Pages.FirstOrDefault(_ => _.PageURL.ToLower() == path.ToLower());

            return page;
        }
    }
}