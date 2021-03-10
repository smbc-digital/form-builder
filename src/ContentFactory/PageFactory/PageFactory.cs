using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Lookup;
using form_builder.Providers.StorageProvider;
using form_builder.TagParsers;
using form_builder.ViewModels;
using Newtonsoft.Json;

namespace form_builder.ContentFactory.PageFactory
{


    public class PageFactory : IPageFactory
    {
        private readonly IPageHelper _pageHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IEnumerable<ITagParser> _tagParsers;
        private readonly IEnumerable<ILookupProvider> _lookupProviders;

        public PageFactory(
            IPageHelper pageHelper,
            IEnumerable<ITagParser> tagParsers,
            IDistributedCacheWrapper distributedCache,
            IEnumerable<ILookupProvider> lookupProviders)
        {
            _pageHelper = pageHelper;
            _tagParsers = tagParsers;
            _distributedCache = distributedCache;
            _lookupProviders = lookupProviders;
        }

        public async Task<FormBuilderViewModel> Build(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string sessionGuid, FormAnswers formAnswers = null, List<object> results = null)
        {
            if (formAnswers == null)
            {
                var cachedAnswers = _distributedCache.GetString(sessionGuid);

                formAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
            }

            _tagParsers.ToList().ForEach(_ => _.Parse(page, formAnswers));

            // HERE : TODO : If this page has "source" in element || "HasDynamicLookUpSource"
            if (page.Elements.Any(x => x.DynamicLookup))
            {
                // Check I have query string
                Answers query = formAnswers.AllAnswers.SingleOrDefault(x => x.QuestionId.Equals("sourceQueryString"));

                if (!string.IsNullOrEmpty((string)query.Response))
                {
                    var elements = page.Elements.Where(x => x.DynamicLookup);
                    foreach (var element in elements)
                    {
                        if (!string.IsNullOrEmpty(element.Properties.SourceObject.Provider))
                        {
                            var lookupProvider = _lookupProviders.Get(element.Properties.SourceObject.Provider);

                            var lookupOptions = await lookupProvider.GetAsync(element.Properties.SourceObject, (string)query.Response);

                            if (!lookupOptions.Any())
                                throw new Exception("test");

                            element.Properties.Options.AddRange(lookupOptions);
                        }
                    }
                }
            }

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
            return result;
        }
    }
}