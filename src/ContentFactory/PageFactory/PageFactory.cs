using form_builder.Configuration;
using form_builder.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.TagParsers;
using form_builder.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.ContentFactory.PageFactory
{

    public class PageFactory : IPageFactory
    {
        private readonly IPageHelper _pageHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IEnumerable<ITagParser> _tagParsers;
        private readonly PreviewModeConfiguration _previewModeConfiguration;

        public PageFactory(
            IPageHelper pageHelper,
            IEnumerable<ITagParser> tagParsers,
            IOptions<PreviewModeConfiguration> previewModeConfiguration,
            IDistributedCacheWrapper distributedCache)
        {
            _pageHelper = pageHelper;
            _tagParsers = tagParsers;
            _distributedCache = distributedCache;
            _previewModeConfiguration = previewModeConfiguration.Value;
        }

        public async Task<FormBuilderViewModel> Build(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string sessionGuid, FormAnswers formAnswers = null, List<object> results = null)
        {
            if (formAnswers is null)
            {
                var cachedAnswers = _distributedCache.GetString(sessionGuid);

                formAnswers = cachedAnswers is null
                    ? new FormAnswers { Pages = new List<PageAnswers>(), FormName = baseForm.BaseURL }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
            }

            _tagParsers.ToList().ForEach(_ => _.Parse(page, formAnswers));

            var result = await _pageHelper.GenerateHtml(page, viewModel, baseForm, sessionGuid, formAnswers, results);
            result.Path = page.PageSlug;
            result.FormName = baseForm.FormName;
            result.PageTitle = page.Title;
            result.FeedbackForm = baseForm.FeedbackForm;
            result.FeedbackPhase = baseForm.FeedbackPhase;
            result.HideBackButton = (viewModel.IsAutomatic() || viewModel.IsManual()) ? false : page.HideBackButton;
            result.BreadCrumbs = baseForm.BreadCrumbs;
            result.DisplayBreadCrumbs = page.DisplayBreadCrumbs;
            result.StartPageUrl = baseForm.StartPageUrl;
            result.IsInPreviewMode = _previewModeConfiguration.IsEnabled && baseForm.BaseURL.StartsWith(PreviewConstants.PREVIEW_MODE_PREFIX);
            return result;
        }
    }
}