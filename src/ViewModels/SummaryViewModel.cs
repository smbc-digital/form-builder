using form_builder.Models;
using System.Collections.Generic;

namespace form_builder.ViewModels
{
    public class SummaryViewModel
    {
        public string FormName { get; set; }

        public string FeedbackPhase { get; set; }

        public string FeedbackFormUrl { get; set; }

        public FormAnswers FormAnswers {get; set;}

        public string Reference { get; set; }

        public string PageContent { get; set; }

        public string StartPageUrl { get; set; }

        public bool HideBackButton { get; set; }

        public string PageTitle { get; set; }
        public bool DisplayBreadcrumbs { get; set; }

        public List<Breadcrumb> Breadcrumbs { get; set; }
    }
}
