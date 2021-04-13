using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.ViewModels
{
    public class SummarySectionViewModel
    {
        public List<SummarySection> Sections = new List<SummarySection>();
        public bool AllowEditing { get; set; }
    }

    public class SummarySection
    {
        public string Title { get; set; }
        public bool DisplaySectionHeading => !string.IsNullOrEmpty(Title);
        public List<PageSummary> Pages { get; set; }
    }
}