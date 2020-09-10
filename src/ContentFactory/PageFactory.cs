using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.ContentFactory
{
    public interface IPageFactory
    {
        Task<FormBuilderViewModel> Build(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string sessionGuid, List<object> results = null);
    }

    public class PageFactory : IPageFactory
    {
        private readonly IPageHelper _pageHelper;

        public PageFactory(IPageHelper pageHelper)
        {
            _pageHelper = pageHelper;
        }

        public async Task<FormBuilderViewModel> Build(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string sessionGuid, List<object> results = null)
        {
            var result = await _pageHelper.GenerateHtml(page, viewModel, baseForm, sessionGuid, results);
            result.Path = page.PageSlug;
            result.FormName = baseForm.FormName;
            result.PageTitle = page.Title;
            result.FeedbackForm = baseForm.FeedbackForm;
            result.FeedbackPhase = baseForm.FeedbackPhase;
            result.HideBackButton = (viewModel.IsAutomatic() || viewModel.IsManual()) ? false : page.HideBackButton;
            result.BreadCrumbs = baseForm.BreadCrumbs;
            result.DisplayBreadCrumbs = page.DisplayBreadCrumbs;
            result.StartPageUrl = baseForm.StartPageUrl;
            return result;
        }
    }
}