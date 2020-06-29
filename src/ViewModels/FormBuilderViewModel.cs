
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

        public string StartFormUrl { get; set; }

        public bool HideBackButton { get; set; }
    }
}