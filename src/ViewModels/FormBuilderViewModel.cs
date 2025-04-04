using form_builder.Models;

namespace form_builder.ViewModels
{
    public class FormBuilderViewModel
    {
        public string RawHTML { get; set; }

        public string Path { get; set; }

        public string FeedbackForm { get; set; }

        public string FeedbackPhase { get; set; }

        public string FormName { get; set; }

        public string PageTitle { get; set; }

        public string StartPageUrl { get; set; }

        public bool HideBackButton { get; set; }

        public bool DisplayBreadCrumbs { get; set; }

        public bool Embeddable { get; set; }

        public List<Breadcrumb> BreadCrumbs { get; set; }

        public bool IsInPreviewMode { get; set; }

        public string UnavailableReason { get; set; }
    }
}