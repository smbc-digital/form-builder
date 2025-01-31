using form_builder.Models;

namespace form_builder.ViewModels
{
    public class PaymentViewModel
    {
        public string FeedbackForm { get; set; }
        public string FeedbackPhase { get; set; }
        public string FormName { get; set; }
        public string PageTitle { get; set; }
        public string Reference { get; set; }
        public string StartPageUrl { get; set; }
        public bool Embeddable { get; set; }
        public bool HideBackButton { get; set; } = false;
        public bool DisplayBreadCrumbs { get; set; }
        public List<Breadcrumb> BreadCrumbs { get; set; }
        public bool IsInPreviewMode { get; set; }
        public string PaymentIssueButtonUrl { get; set; }
        public string PaymentIssueButtonLabel { get; set; }
    }
}