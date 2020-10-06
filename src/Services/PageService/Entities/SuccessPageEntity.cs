using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Services.PageService.Entities
{
    public class SuccessPageEntity
    {
        public string ViewName { get; set; } = "Success";

        public FormAnswers FormAnswers { get; set; }

        public string CaseReference { get; set; }

        public string FeedbackFormUrl { get; set; }

        public string FeedbackPhase { get; set; }

        public string HtmlContent { get; set; }

        public string FormName { get; set; }

        public string StartPageUrl { get; set; }

        public string PageTitle { get; set; }

        public string BannerTitle { get; set; }

        public string LeadingParagraph { get; set; }

        public bool DisplayBreadcrumbs { get; set; }

        public List<Breadcrumb> Breadcrumbs {get;set;}

    }
}
