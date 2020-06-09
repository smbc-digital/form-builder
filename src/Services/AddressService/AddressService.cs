﻿using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Providers.Address;
using form_builder.Extensions;
using form_builder.Constants;

namespace form_builder.Services.AddressService
{
    public interface IAddressService
    {
        Task<ProcessRequestEntity> ProcessAddress(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path);
    }

    public class AddressService : IAddressService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IPageHelper _pageHelper;
        private readonly IEnumerable<IAddressProvider> _addressProviders;

        public AddressService(
            IDistributedCacheWrapper distributedCache,
            IPageHelper pageHelper,
            IEnumerable<IAddressProvider> addressProviders)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _addressProviders = addressProviders;
        }

        public async Task<ProcessRequestEntity> ProcessAddress(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

            switch (subPath)
            {
                case LookUpConstants.Manual:
                    return await ProcesssManualAddress(viewModel, currentPage, baseForm, guid, path);
                case LookUpConstants.Automatic:
                    return await ProcesssAutomaticAddress(viewModel, currentPage, baseForm, guid, path);
                default:
                    return await ProcesssSearchAddress(viewModel, currentPage, baseForm, guid, path);
            }
        }

        private async Task<ProcessRequestEntity> ProcesssManualAddress(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

            if (!currentPage.IsValid)
            {
                var cachedAnswers = _distributedCache.GetString(guid);

                var convertedAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var cachedSearchResults = convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>;

                var model = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, cachedSearchResults.ToList());
                model.Path = currentPage.PageSlug;
                model.FormName = baseForm.FormName;
                model.PageTitle = currentPage.Title;
                model.BaseURL = baseForm.BaseURL;

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = model
                };
            }

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        private async Task<ProcessRequestEntity> ProcesssAutomaticAddress(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            var cachedAnswers = _distributedCache.GetString(guid);

            var convertedAnswers = cachedAnswers == null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var addressElement = currentPage.Elements.Where(_ => _.Type == EElementType.Address).FirstOrDefault();

            var postcode = (string)convertedAnswers
                        .Pages
                        .FirstOrDefault(_ => _.PageSlug == path)
                        .Answers
                        .FirstOrDefault(_ => _.QuestionId == $"{addressElement.Properties.QuestionId}-postcode")
                        .Response;

            var address = (string)viewModel[$"{addressElement.Properties.QuestionId}-address"];

            if (currentPage.IsValid && addressElement.Properties.Optional && string.IsNullOrEmpty(postcode))
            {
                _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            if (currentPage.IsValid && addressElement.Properties.Optional && string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(postcode))
            {
                _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            if (!currentPage.IsValid)
            {
                var cachedSearchResults = convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>;
                
                var model = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, cachedSearchResults.ToList());
                model.Path = currentPage.PageSlug;
                model.FormName = baseForm.FormName;
                model.PageTitle = currentPage.Title;

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = model
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        private async Task<ProcessRequestEntity> ProcesssSearchAddress(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            var cachedAnswers = _distributedCache.GetString(guid);

            var convertedAnswers = cachedAnswers == null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var addressElement = currentPage.Elements.Where(_ => _.Type == EElementType.Address).FirstOrDefault();

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid);
                formModel.Path = currentPage.PageSlug;
                formModel.FormName = baseForm.FormName;
                formModel.PageTitle = currentPage.Title;

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel
                };
            }

            var postcode = (string)viewModel[$"{addressElement.Properties.QuestionId}-postcode"];

            if (addressElement.Properties.Optional && string.IsNullOrEmpty(postcode))
            {
                _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            var addressResults = new List<object>();
            try
            {
                var searchResult = await _addressProviders.Get(addressElement.Properties.AddressProvider).SearchAsync(postcode);
                addressResults = searchResult.ToList<object>();
            }
            catch (Exception e)
            {
                throw new ApplicationException($"AddressController: An exception has occured while attempting to perform postcode lookup", e);
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
            _pageHelper.SaveFormData($"{path}{LookUpConstants.SearchResultsKeyPostFix}", addressResults, guid);

            if (!addressResults.Any())
            {
                return new ProcessRequestEntity
                {
                    RedirectToAction = true,
                    RedirectAction = "Index",
                    RouteValues = new
                    {
                        form = baseForm.BaseURL,
                        path,
                        subPath = LookUpConstants.Manual
                    }
                };
            }

            return new ProcessRequestEntity
            {
                RedirectToAction = true,
                RedirectAction = "Index",
                RouteValues = new
                {
                    form = baseForm.BaseURL,
                    path,
                    subPath = LookUpConstants.Automatic
                }
            };
        }
    }
}
