using form_builder.Models;

namespace form_builder.ViewModels
{
    public class SummarySectionsViewModel
    {
        public List<SummarySection> Sections = new List<SummarySection>();
        public bool AllowEditing { get; set; }
    }

    public class SummarySection
    {
        public string Title { get; set; }
        public List<PageSummary> Pages { get; set; }
        public bool ContainsPages => Pages is not null && Pages.Any();
        public bool DisplaySectionHeading => !string.IsNullOrEmpty(Title);
    }
}