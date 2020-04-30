using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models
{
    public class SuccessViewModel
    {
        public string FormName { get; set; }

        public string SecondaryHeader { get; set; }

        public FormAnswers FormAnswers {get; set;}

        public string Reference { get; set; }

        public string PageContent { get; set; }

        public string StartFormUrl { get; set; }

        public bool HideBackButton => true;

        public string PageTitle { get; set; }
    }
}
