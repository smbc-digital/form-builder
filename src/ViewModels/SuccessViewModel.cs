using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.ViewModels
{
    public class SuccessViewModel
    {
        public string FormName { get; set; }

        public string FeedbackPhase { get; set; }

        public string FeedbackFormUrl { get; set; }

        public FormAnswers FormAnswers {get; set;}
        public string Reference { get; set; }

        public string PageContent { get; set; }

        public string StartPageUrl { get; set; }

        public bool HideBackButton => true;

        public string PageTitle { get; set; }

        public string BannerTitle { get; set; }

        public string LeadingParagraph { get; set; }

        public bool DisplayBreadcrumbs { get; set; }

        public List<Breadcrumb> Breadcrumbs{get; set;}
    }
}
