using form_builder.Models;

namespace form_builder.ViewModels
{
    public class DataStructureViewModel
    {
        public string FormName { get; set; }

        public string FeedbackPhase { get; set; }

        public string FeedbackForm { get; set; }

        public string StartPageUrl { get; set; }

        public bool Embeddable { get; set; }

        public bool HideBackButton => true;

        public string PageTitle { get; set; }

        public bool DisplayBreadcrumbs { get; set; }

        public List<Breadcrumb> Breadcrumbs { get; set; }

        public bool IsInPreviewMode { get; set; }

        public object DataStructure { get; set; }

        public List<LanguageObjectExample> ObjectExamples { get; set; }
    }

    public class LanguageObjectExample
    {
        public string LanguageName { get; set; }
        public string ObjectCode { get; set;  }
    }
}
