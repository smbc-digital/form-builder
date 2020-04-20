﻿using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Gateways.OrganisationServiceGateway;
using StockportGovUK.NetStandard.Models.Organisation;
using form_builder.Providers.Organisation;
using form_builder.Extensions;
using form_builder.Models.Elements;

namespace form_builder.Services.OrganisationService
{
    public interface IOrganisationService
    {
        Task<ProcessRequestEntity> ProcesssOrganisation(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path);
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

        public async Task<ProcessRequestEntity> ProcesssOrganisation(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var journey = (string)viewModel["OrganisationStatus"];
            var organisationResults = new List<OrganisationSearchResult>();

            if ((!currentPage.IsValid && journey == "Select") || (currentPage.IsValid && journey == "Search"))
            {
                var cachedAnswers = _distributedCache.GetString(guid);
                var convertedAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var organisationElement = (Organisation)currentPage.Elements.Where(_ => _.Type == EElementType.Organisation).FirstOrDefault();

                var searchTerm = journey == "Select"
                    ? (string)convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug == path).Answers.FirstOrDefault(_ => _.QuestionId == organisationElement.OrganisationSearchQuestionId).Response
                    : (string)viewModel[organisationElement.OrganisationSearchQuestionId];

                var organisation = journey != "Select"
                    ? string.Empty
                    : (string)viewModel[organisationElement.OrganisationSelectQuestionId];

                var emptySearchTerm = string.IsNullOrEmpty(searchTerm);
                var emptyOrganisation = string.IsNullOrEmpty(organisation);

                if (currentPage.IsValid && organisationElement.Properties.Optional && emptySearchTerm)
                {
                    _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                }

                if (currentPage.IsValid && organisationElement.Properties.Optional && emptyOrganisation && !emptySearchTerm && journey == "Select")
                {
                    _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                }

                try
                {
                    var result = await _organisationProviders.Get(organisationElement.Properties.OrganisationProvider).SearchAsync(searchTerm);
                    organisationResults = result.ToList();
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"OrganisationService.ProcesssOrganisation:: An exception has occured while attempting to perform organisation lookup, Exception: {e.Message}");
                }
            }

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, null, organisationResults);
                formModel.Path = currentPage.PageSlug;
                formModel.OrganisationStatus = journey;
                formModel.FormName = baseForm.FormName;
                formModel.PageTitle = currentPage.Title;
                formModel.HideBackButton = currentPage.HideBackButton;

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel,
                    ViewName = "../Organisation/Index"
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
            return await _pageHelper.ProcessOrganisationJourney(journey, currentPage, viewModel, baseForm, guid, organisationResults);
        }
    }
}
