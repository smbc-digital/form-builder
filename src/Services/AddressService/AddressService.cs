using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Providers.Address;
using form_builder.Extensions;
using form_builder.Models.Elements;

namespace form_builder.Services.AddressService
{
    public interface IAddressService
    {
        Task<ProcessRequestEntity> ProcesssAddress(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path, string subPath);
    }

    public class AddressService : IAddressService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IPageHelper _pageHelper;
        private readonly IEnumerable<IAddressProvider> _addressProviders;

        public AddressService(IDistributedCacheWrapper distributedCache, IPageHelper pageHelper, IEnumerable<IAddressProvider> addressProviders)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _addressProviders = addressProviders;
        }

        public async Task<ProcessRequestEntity> ProcesssAddress(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path,
            string subPath)
        {
            var cachedAnswers = _distributedCache.GetString(guid);

            var convertedAnswers = cachedAnswers == null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var addressElement = currentPage.Elements.Where(_ => _.Type == EElementType.Address).FirstOrDefault();

            switch (subPath)
            {
                case "manual":
                    _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

                    if (!currentPage.IsValid)
                    {
                        var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid);
                        formModel.Path = currentPage.PageSlug;
                        formModel.FormName = baseForm.FormName;
                        formModel.PageTitle = currentPage.Title;
                        formModel.BaseURL = baseForm.BaseURL;

                        return new ProcessRequestEntity
                        {
                            Page = currentPage,
                            ViewModel = formModel
                        };
                    }

                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                case "automatic":
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

                    if (!currentPage.IsValid)
                    {
                        var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, null, null, subPath, addressResults);
                        formModel.Path = currentPage.PageSlug;
                        formModel.FormName = baseForm.FormName;
                        formModel.PageTitle = currentPage.Title;

                        return new ProcessRequestEntity
                        {
                            Page = currentPage,
                            ViewModel = formModel
                        };
                    }

                    _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
                    _pageHelper.SaveFormData($"{addressElement.Properties.QuestionId}-srcount", addressResults.Count, guid);
                    
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                default:
                    if (!currentPage.IsValid)
                    {
                        var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, new List<AddressSearchResult>());
                        formModel.Path = currentPage.PageSlug;
                        formModel.FormName = baseForm.FormName;
                        formModel.PageTitle = currentPage.Title;

                        return new ProcessRequestEntity
                        {
                            Page = currentPage,
                            ViewModel = formModel
                        };
                    }

                    postcode = (string)viewModel[$"{addressElement.Properties.QuestionId}-postcode"];

                    if (addressElement.Properties.Optional && string.IsNullOrEmpty(postcode))
                    {
                        _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
                        return new ProcessRequestEntity
                        {
                            Page = currentPage
                        };
                    }

                    addressResults = new List<object>();
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
                    _pageHelper.SaveFormData($"{path}-sr", addressResults, guid);

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
                                subPath = "manual"
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
                            subPath = "automatic"
                        }
                    };
            }
        }
    }
}
