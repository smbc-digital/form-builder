using form_builder.Models;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using form_builder.Models;
>>>>>>> design-system

namespace form_builder.ViewModels
{
    public class SuccessViewModel
    {
        public string FormName { get; set; }

        public string FeedbackPhase { get; set; }

        public string FeedbackFormUrl { get; set; }

<<<<<<< HEAD
        public FormAnswers FormAnswers { get; set; }
=======
        public FormAnswers FormAnswers {get; set;}
>>>>>>> design-system

        public string Reference { get; set; }

        public string PageContent { get; set; }

<<<<<<< HEAD
        public string StartFormUrl { get; set; }
=======
        public string StartPageUrl { get; set; }
>>>>>>> design-system

        public bool HideBackButton => true;

        public string PageTitle { get; set; }

        public string BannerTitle { get; set; }

        public string LeadingParagraph { get; set; }
<<<<<<< HEAD

=======
      
>>>>>>> design-system
        public bool DisplayBreadcrumbs { get; set; }

        public List<Breadcrumb> Breadcrumbs{get; set;}
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> design-system
