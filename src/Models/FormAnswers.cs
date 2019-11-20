using System.Collections.Generic;

namespace form_builder.Models
{
    public class PageAnswers
    {
        public string PageUrl { get; set; }
        public List<Answers> Answers { get; set; }
    }

    public class FormAnswers
    {
        public string Path { get; set; }

        public string AddressStatus { get; set; }

        public List<PageAnswers> Pages { get; set; }
    }
}