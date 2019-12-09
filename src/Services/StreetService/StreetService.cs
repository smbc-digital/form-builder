using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Street;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Services.StreetService
{
    public interface IStreetService
    {
        Task<ProcessPageEntity> ProcessStreet(Dictionary<string, string> viewModel, Page currentPage, FormSchema baseForm, string guid, string path);
    }

    public class StreetService : IStreetService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IPageHelper _pageHelper;
        private readonly IEnumerable<IStreetProvider> _streetProviders;

        public StreetService(IDistributedCacheWrapper distributedCache, IEnumerable<IStreetProvider> streetProviders, IPageHelper pageHelper)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _streetProviders = streetProviders;
        }

        public async Task<ProcessPageEntity> ProcessStreet(Dictionary<string, string> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var journey = viewModel["StreetStatus"];
            var streetResults = new List<AddressSearchResult>();

            var cachedAnswers = _distributedCache.GetString(guid);
            var streetElement = currentPage.Elements.Where(_ => _.Type == EElementType.Street).FirstOrDefault();
            var convertedAnswers = cachedAnswers == null
                        ? new FormAnswers { Pages = new List<PageAnswers>() }
                        : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
            var street = journey == "Select"
                ? convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug == path).Answers.FirstOrDefault(_ => _.QuestionId == $"{streetElement.Properties.QuestionId}-street").Response
                : viewModel[$"{streetElement.Properties.QuestionId}-street"];

            if (currentPage.IsValid && streetElement.Properties.Optional && string.IsNullOrEmpty(street))
            {
                _pageHelper.SaveAnswers(viewModel, guid);
                return new ProcessPageEntity
                {
                    Page = currentPage
                };
            }

            if ((!currentPage.IsValid && journey == "Select") || (currentPage.IsValid && journey == "Search"))
            {
                var provider = _streetProviders.ToList()
                    .Where(_ => _.ProviderName == streetElement.Properties.StreetProvider)
                    .FirstOrDefault();

                if (provider == null)
                {
                    throw new ApplicationException($"No street provider configure for {streetElement.Properties.AddressProvider}");
                }

                try
                {
                    var result = await provider.SearchAsync(street);
                    streetResults = result.ToList();
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"StreetController: An exception has occured while attempting to perform street lookup, Exception: {e.Message}");
                }
            }

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, streetResults);
                formModel.Path = currentPage.PageSlug;
                formModel.StreetStatus = journey;
                formModel.FormName = baseForm.FormName;

                return new ProcessPageEntity
                {
                    Page = currentPage,
                    ViewModel = formModel,
                    ViewName = "../Street/Index"
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid);
            return await _pageHelper.ProcessStreetAndAddressJourney(journey, currentPage, viewModel, baseForm, guid, streetResults, false);
        }
    }
}
