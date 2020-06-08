﻿using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Street;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.StreetServiceGateway;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Street;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Services.StreetService
{
    public interface IStreetService
    {
        Task<ProcessRequestEntity> ProcessStreet(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path);
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

        public async Task<ProcessRequestEntity> ProcessStreet(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var journey = (string)viewModel["StreetStatus"];
            var streetResults = new List<AddressSearchResult>();

            var cachedAnswers = _distributedCache.GetString(guid);
            var streetElement = currentPage.Elements.Where(_ => _.Type == EElementType.Street).FirstOrDefault();
            var convertedAnswers = cachedAnswers == null
                        ? new FormAnswers { Pages = new List<PageAnswers>() }
                        : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
            var street = journey == "Select"
                ? (string) convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug == path).Answers.FirstOrDefault(_ => _.QuestionId == $"{streetElement.Properties.QuestionId}-street").Response
                : (string) viewModel[$"{streetElement.Properties.QuestionId}-street"];

            if (currentPage.IsValid && streetElement.Properties.Optional && string.IsNullOrEmpty(street))
            {
                _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            if ((!currentPage.IsValid && journey == "Select") || (currentPage.IsValid && journey == "Search"))
            {
                try
                {
                    var result = await _streetProviders.Get(streetElement.Properties.StreetProvider).SearchAsync(street);
                    streetResults = result.ToList();
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"StreetService::ProcessStreet: An exception has occured while attempting to perform street lookup, Exception: {e.Message}");
                }
            }

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, null); // NOTE:: null is streetResults
                formModel.Path = currentPage.PageSlug;
                formModel.StreetStatus = journey;
                formModel.FormName = baseForm.FormName;
                formModel.PageTitle = currentPage.Title;

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel,
                    ViewName = "../Street/Index"
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
            return await _pageHelper.ProcessStreetJourney(journey, currentPage, viewModel, baseForm, guid, streetResults);
        }
    }
}
