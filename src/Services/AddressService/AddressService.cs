using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.AddressService;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Enums;

namespace form_builder.Services.AddressService
{
    public interface IAddressService
    {
        Task<ProcessRequestEntity> ProcesssAddress(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path);
    }

    public class AddressService : IAddressService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IPageHelper _pageHelper;
        private readonly IAddressServiceGateway _addressServiceGateway;

        public AddressService(IDistributedCacheWrapper distributedCache, IAddressServiceGateway addressServiceGateway, IPageHelper pageHelper)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _addressServiceGateway = addressServiceGateway;
        }

        public async Task<ProcessRequestEntity> ProcesssAddress(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var journey = (string)viewModel["AddressStatus"];
            var addressResults = new List<AddressSearchResult>();

            if ((!currentPage.IsValid && journey == "Select") || (currentPage.IsValid && journey == "Search"))
            {
                var cachedAnswers = _distributedCache.GetString(guid);
                var convertedAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var addressElement = currentPage.Elements.Where(_ => _.Type == EElementType.Address).FirstOrDefault();

                var postcode = journey == "Select"
                    ? (string) convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug == path).Answers.FirstOrDefault(_ => _.QuestionId == $"{addressElement.Properties.QuestionId}-postcode").Response
                    : (string) viewModel[$"{addressElement.Properties.QuestionId}-postcode"];

                var address = journey != "Select"
                    ? string.Empty
                    : (string)viewModel[$"{addressElement.Properties.QuestionId}-address"];

                var emptyPostcode = string.IsNullOrEmpty(postcode);
                var emptyAddress = string.IsNullOrEmpty(address);
                var truthstatement = emptyPostcode ^ emptyAddress;

                if (currentPage.IsValid && addressElement.Properties.Optional && emptyPostcode)
                {
                    _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null);
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                }

                if (currentPage.IsValid && addressElement.Properties.Optional && emptyAddress && !emptyPostcode && journey == "Select")
                {
                    _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null);
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                }

                try
                {
                    var result = await _addressServiceGateway.SearchAsync(new AddressSearch { AddressProvider = (EAddressProvider)System.Enum.Parse(typeof(EAddressProvider), addressElement.Properties.AddressProvider), SearchTerm = postcode });
                    addressResults = result.ResponseContent.ToList();
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"AddressController: An exception has occured while attempting to perform postcode lookup, Exception: {e.Message}");
                }
            }

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, addressResults);
                formModel.Path = currentPage.PageSlug;
                formModel.AddressStatus = journey;
                formModel.FormName = baseForm.FormName;
                formModel.PageTitle = currentPage.Title;

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel,
                    ViewName = "../Address/Index"
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null);
            return await _pageHelper.ProcessAddressJourney(journey, currentPage, viewModel, baseForm, guid, addressResults);
        }
    }
}
