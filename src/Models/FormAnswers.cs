using System.Collections.Generic;

namespace form_builder.Models
{
    public class PageAnswers
    {
        public string PageSlug { get; set; }
        public List<Answers> Answers { get; set; }
    }

    public class FormAnswers
    {
        public string FormName { get; set; }
        public string Path { get; set; }

        public List<PageAnswers> Pages { get; set; }
    }
}