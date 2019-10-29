using System.Collections.Generic;

namespace form_builder.Models
{
    public class FormAnswers
    {
        public string PageUrl { get; set; }
        public List<Answers> Answers { get; set; }
    }
}