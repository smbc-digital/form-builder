using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.ContentFactory.PageFactory
{
    public interface IPageFactory
    {
        Task<FormBuilderViewModel> Build(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string sessionGuid, FormAnswers formAnswers = null, List<object> results = null);
    }
}
