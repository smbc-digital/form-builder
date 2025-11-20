using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.Services.PageService.Entities
{
    public class PageEntity
    {
        public string ViewName { get; set; } = "Index";

        public Page Page { get; set; }

        public FormBuilderViewModel ViewModel { get; set; }
    }

    public class ProcessPageEntity : PageEntity
    {
        public bool ShouldRedirect { get; set; }

        public bool RequiresAccessKey { get; set; }

        public string TargetPage { get; set; } = string.Empty;
    }

    public class ProcessRequestEntity : PageEntity
    {
        public bool UseGeneratedViewModel { get; set; }

        public bool RedirectToAction { get; set; }

        public string RedirectAction { get; set; } = string.Empty;

        public object RouteValues { get; set; }
    }
}