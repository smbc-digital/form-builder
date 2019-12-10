using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.Services.PageService.Entities
{
    public class ProcessPageEntity
    {
        public Page Page { get; set; }

        public FormBuilderViewModel ViewModel { get; set; }

        public bool UseGeneratedViewModel { get; set; } = false;

        public string ViewName { get; set; } = "Index";
    }
}