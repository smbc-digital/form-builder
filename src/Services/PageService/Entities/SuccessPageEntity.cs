using form_builder.Models;

namespace form_builder.Services.PageService.Entities
{
    public class SuccessPageEntity
    {
        public string ViewName { get; set; } = "Success";
        public FormAnswers FormAnswers { get; set; }
        public string FeedbackFormUrl {get;set;}
        public string HtmlContent { get; set; }
        public string FormName { get; set; }
        public string SecondaryHeader { get; set; }
        public string StartFormUrl { get; set; }
        public string PageTitle { get; set; }
    }
}

