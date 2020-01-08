using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.Organisation;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Models.Verint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Services.OrganisationService
{
    public interface IOrganisationService
    {
        Task<ProcessRequestEntity> ProcesssOrganisation(Dictionary<string, string> viewModel, Page currentPage, FormSchema baseForm, string guid, string path);
    }

    public class OrganisationService : IOrganisationService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IPageHelper _pageHelper;
        private readonly IEnumerable<IOrganisationProvider> _organisationProviders;

        public OrganisationService(IDistributedCacheWrapper distributedCache, IEnumerable<IOrganisationProvider> organisationProviders, IPageHelper pageHelper)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _organisationProviders = organisationProviders;
        }

        public async Task<ProcessRequestEntity> ProcesssOrganisation(Dictionary<string, string> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var journey = viewModel["OrganisationStatus"];
            var organisationResults = new List<Organisation>();

            if ((!currentPage.IsValid && journey == "Select") || (currentPage.IsValid && journey == "Search"))
            {
                var cachedAnswers = _distributedCache.GetString(guid);
                var convertedAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var organisationElement = currentPage.Elements.Where(_ => _.Type == EElementType.Organisation).FirstOrDefault();
                var provider = _organisationProviders.ToList()
                    .Where(_ => _.ProviderName == organisationElement.Properties.OrganisationProvider)
                    .FirstOrDefault();

                if (provider == null)
                {
                    throw new ApplicationException($"OrganisationService.ProcesssOrganisation:: No address provider configure for {organisationElement.Properties.OrganisationProvider}");
                }

                var searchTerm = journey == "Select"
                    ? convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug == path).Answers.FirstOrDefault(_ => _.QuestionId == $"{organisationElement.Properties.QuestionId}-searchterm-organisation").Response
                    : viewModel[$"{organisationElement.Properties.QuestionId}-searchterm-organisation"];

                var organisation = journey != "Select"
                    ? string.Empty
                    : viewModel[$"{organisationElement.Properties.QuestionId}-organisation"];

                var emptySearchTerm = string.IsNullOrEmpty(searchTerm);
                var emptyOrganisation = string.IsNullOrEmpty(organisation);

                if (currentPage.IsValid && organisationElement.Properties.Optional && emptySearchTerm)
                {
                    _pageHelper.SaveAnswers(viewModel, guid);
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                }

                if (currentPage.IsValid && organisationElement.Properties.Optional && emptyOrganisation && !emptySearchTerm && journey == "Select")
                {
                    _pageHelper.SaveAnswers(viewModel, guid);
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                }

                try
                {
                    var result = await provider.SearchAsync(searchTerm);
                    organisationResults = result.ToList();
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"OrganisationService.ProcesssOrganisation:: An exception has occured while attempting to perform postcode lookup, Exception: {e.Message}");
                }
            }

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, null, organisationResults);
                formModel.Path = currentPage.PageSlug;
                formModel.AddressStatus = journey;
                formModel.FormName = baseForm.FormName;

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel,
                    ViewName = "../Organisation/Index"
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid);
            return await _pageHelper.ProcessStreetAndAddressJourney(journey, currentPage, viewModel, baseForm, guid, null, organisationResults, false);
        }
    }
}
