using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace form_builder.ContentFactory
{
    public interface IPageFactory
    {
        Task<FormBuilderViewModel> Build(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string sessionGuid, List<object> results = null);
    }
    
    public class PageFactory : IPageFactory
    {
        private readonly IPageHelper _pageHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _environment;

        public PageFactory(IPageHelper pageHelper, IHttpContextAccessor httpContextAccessor, IHostingEnvironment environment)
        {
            _pageHelper = pageHelper;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public async Task<FormBuilderViewModel> Build(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string sessionGuid, List<object> results = null)
        {
            var result = await _pageHelper.GenerateHtml(page, viewModel, baseForm, sessionGuid, results);         
            result.Path = page.PageSlug;
            result.FormName = baseForm.FormName;
            result.PageTitle = page.Title;
            result.FeedbackForm = baseForm.FeedbackForm;
            result.FeedbackPhase = baseForm.FeedbackPhase;
            result.HideBackButton = page.HideBackButton;
            result.StartFormUrl = _environment.EnvironmentName == "local" || _environment.EnvironmentName == "uitest"
                ? $"https://{_httpContextAccessor.HttpContext.Request.Host}/{baseForm.BaseURL}/{baseForm.StartPageSlug}"
                : $"https://{_httpContextAccessor.HttpContext.Request.Host}/v2/{baseForm.BaseURL}/{baseForm.StartPageSlug}";
            
            return result;
        }
    }
}