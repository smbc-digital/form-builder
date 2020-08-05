using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.Services.PayService.Entities
{
    public class PageEntity
    {
        public string ViewName { get; set; } = "Index";

        public Page Page { get; set; }

        public FormBuilderViewModel ViewModel { get; set; }
    }

    public class ProcessPaymentEntity : PageEntity
    {
        public bool ShouldRedirect { get; set; } = false;

        public string TargetPage { get; set; } = string.Empty;
    }
}