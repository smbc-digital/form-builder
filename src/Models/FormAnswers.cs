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

        public string CaseReference {get;set;}

        public string StartPageUrl { get; set; }

        public Dictionary<string, object> FormData { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> AdditionalFormData { get; set; } = new Dictionary<string, object>();

        public List<PageAnswers> Pages { get; set; }

        public string AppointmentLocation { get; set; }
    }
}