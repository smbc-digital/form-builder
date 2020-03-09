using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using form_builder.Services.FileUploadService;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        void HasDuplicateQuestionIDs(List<Page> pages, string formName);
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressSearchResults = null, List<OrganisationSearchResult> organisationSearchResults = null);
        void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IFormFileCollection file);
        Task<ProcessRequestEntity> ProcessOrganisationJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<OrganisationSearchResult> organisationResults);
        Task<ProcessRequestEntity> ProcessStreetJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults);
        Task<ProcessRequestEntity> ProcessAddressJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults);
        void CheckForInvalidQuestionOrTargetMappingValue(List<Page> pages, string formName);
        Task CheckForPaymentConfiguration(List<Page> pages, string formName);
        void CheckForEmptyBehaviourSlugs(List<Page> pages, string formName);
        void CheckForCurrentEnvironmentSubmitSlugs(List<Page> pages, string formName);
        void CheckSubmitSlugsHaveAllProperties(List<Page> pages, string formName);
        void CheckForAcceptedFileUploadFileTypes(List<Page> pages, string formName);
        void SaveFormData(string key, object value, string guid);
    }

    public class PageHelper : IPageHelper
    {
        private readonly IViewRender _viewRender;
        private readonly IElementHelper _elementHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DisallowedAnswerKeysConfiguration _disallowedKeys;
        private readonly IHostingEnvironment _enviroment;
        private readonly DistributedCacheExpirationConfiguration _distrbutedCacheExpirationConfiguration;
        private readonly ICache _cache;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly IFileUploadService _fileUploadService;

        public PageHelper(IViewRender viewRender, IElementHelper elementHelper, IDistributedCacheWrapper distributedCache, 
            IOptions<DisallowedAnswerKeysConfiguration> disallowedKeys, IHostingEnvironment enviroment, ICache cache, 
            IOptions<DistributedCacheExpirationConfiguration> distrbutedCacheExpirationConfiguration, 
            IEnumerable<IPaymentProvider> paymentProviders, IFileUploadService fileUploadService)
        {
            _viewRender = viewRender;
            _elementHelper = elementHelper;
            _distributedCache = distributedCache;
            _disallowedKeys = disallowedKeys.Value;
            _enviroment = enviroment;
            _cache = cache;
            _distrbutedCacheExpirationConfiguration = distrbutedCacheExpirationConfiguration.Value;
            _paymentProviders = paymentProviders;
            _fileUploadService = fileUploadService;
        }

        public async Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressAndStreetSearchResults = null, List<OrganisationSearchResult> organisationSearchResults = null)
        {
            FormBuilderViewModel formModel = new FormBuilderViewModel();
            if (page.PageSlug.ToLower() != "success")
            {
                formModel.RawHTML += await _viewRender.RenderAsync("H1", new Element { Properties = new BaseProperty { Text = baseForm.FormName } });
            }
            formModel.FeedbackForm = baseForm.FeedbackForm;

            foreach (var element in page.Elements)
            {
                formModel.RawHTML += await element.RenderAsync(_viewRender, _elementHelper, guid, addressAndStreetSearchResults, organisationSearchResults, viewModel, page, baseForm, _enviroment);
            }

            return formModel;
        }

        public void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IFormFileCollection file)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
            {
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            }

            if (convertedAnswers.Pages != null && convertedAnswers.Pages.Any(_ => _.PageSlug == viewModel["Path"].ToLower()))
            {
                convertedAnswers.Pages = convertedAnswers.Pages.Where(_ => _.PageSlug != viewModel["Path"].ToLower()).ToList();
            }

            var answers = new List<Answers>();

            foreach (var item in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Contains(item.Key))
                {
                    answers.Add(new Answers { QuestionId = item.Key, Response = item.Value });
                }
            }

            convertedAnswers.Pages.Add(new PageAnswers
            {
                PageSlug = viewModel["Path"].ToLower(),
                Answers = answers
            });

            convertedAnswers.Path = viewModel["Path"];
            convertedAnswers.FormName = form;

            convertedAnswers = _fileUploadService.CollectAnswers(convertedAnswers, file, viewModel);
            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public async Task<ProcessRequestEntity> ProcessStreetJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults)
        {
            switch (journey)
            {
                case "Search":
                    try
                    {
                        var streetViewModel = await GenerateHtml(currentPage, viewModel, baseForm, guid, addressResults, null);
                        streetViewModel.StreetStatus = "Select";
                        streetViewModel.FormName = baseForm.FormName;
                        streetViewModel.PageTitle = currentPage.Title;

                        return new ProcessRequestEntity
                        {
                            Page = currentPage,
                            ViewModel = streetViewModel,
                            UseGeneratedViewModel = true,
                            ViewName = "../Street/Index"
                        };
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"PageHelper.ProcessStreetJourney: An exception has occured while attempting to generate Html, Exception: {e.Message}");
                    };
                case "Select":
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                default:
                    throw new ApplicationException($"PageHelper.ProcessStreetJourney: Unknown journey type");
            }
        }

        public async Task<ProcessRequestEntity> ProcessAddressJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults)
        {
            switch (journey)
            {
                case "Search":
                    try
                    {
                        var adddressViewModel = await GenerateHtml(currentPage, viewModel, baseForm, guid, addressResults, null);
                        adddressViewModel.AddressStatus = "Select";
                        adddressViewModel.FormName = baseForm.FormName;
                        adddressViewModel.PageTitle = currentPage.Title;

                        return new ProcessRequestEntity
                        {
                            Page = currentPage,
                            ViewModel = adddressViewModel,
                            UseGeneratedViewModel = true,
                            ViewName = "../Address/Index"
                        };
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"PageHelper.ProcessAddressJourney: An exception has occured while attempting to generate Html, Exception: {e.Message}");
                    };
                case "Select":
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                default:
                    throw new ApplicationException("PageHelper.ProcessAddressJourney: Unknown journey type");
            }
        }

        public async Task<ProcessRequestEntity> ProcessOrganisationJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<OrganisationSearchResult> organisationResults)
        {
            switch (journey)
            {
                case "Search":
                    try
                    {
                        var organisationViewModel = await GenerateHtml(currentPage, viewModel, baseForm, guid, null, organisationResults);
                        organisationViewModel.OrganisationStatus = "Select";
                        organisationViewModel.FormName = baseForm.FormName;
                        organisationViewModel.PageTitle = currentPage.Title;

                        return new ProcessRequestEntity
                        {
                            Page = currentPage,
                            ViewModel = organisationViewModel,
                            UseGeneratedViewModel = true,
                            ViewName = "../Organisation/Index"
                        };
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"PageHelper.ProcessOrganisationJourney: An exception has occured while attempting to generate Html, Exception: {e.Message}");
                    };
                case "Select":
                    return new ProcessRequestEntity
                    {
                        Page = currentPage
                    };
                default:
                    throw new ApplicationException($"PageHelper.ProcessOrganisationJourney: Unknown journey type");
            }
        }

        public void HasDuplicateQuestionIDs(List<Page> pages, string formName)
        {
            List<string> qIds = new List<string>();
            foreach (var page in pages)
            {
                foreach (var element in page.Elements)
                {
                    if (element.Type != EElementType.H1
                        && element.Type != EElementType.H2
                        && element.Type != EElementType.H3
                        && element.Type != EElementType.H4
                        && element.Type != EElementType.H5
                        && element.Type != EElementType.H6
                        && element.Type != EElementType.Img
                        && element.Type != EElementType.InlineAlert
                        && element.Type != EElementType.P
                        && element.Type != EElementType.Span
                        && element.Type != EElementType.UL
                        && element.Type != EElementType.OL
                        && element.Type != EElementType.Button
                        )
                    {
                        qIds.Add(element.Properties.QuestionId);
                    }
                }
            }

            var hashSet = new HashSet<string>();
            foreach (var id in qIds)
            {
                if (!hashSet.Add(id))
                {
                    throw new ApplicationException($"The provided json '{formName}' has duplicate QuestionIDs");
                }
            }
        }

        public void CheckForEmptyBehaviourSlugs(List<Page> pages, string formName)
        {
            List<Behaviour> behaviours = new List<Behaviour>();

            foreach (var page in pages)
            {
                if (page.Behaviours != null)
                {
                    foreach (var behaviour in page.Behaviours)
                    {
                        behaviours.Add(behaviour);
                    }
                }
            }

            foreach (var item in behaviours)
            {
                if (string.IsNullOrEmpty(item.PageSlug) && (item.SubmitSlugs == null || item.SubmitSlugs.Count == 0))
                {
                    throw new ApplicationException($"Incorrectly configured behaviour slug was discovered in {formName} form");
                }
            }
        }

        public async Task CheckForPaymentConfiguration(List<Page> pages, string formName)
        {
            var containsPayment = pages.Where(x => x.Behaviours != null)
                .SelectMany(x => x.Behaviours)
                .Any(x => x.BehaviourType == EBehaviourType.SubmitAndPay);

            if (!containsPayment)
                return;

            var paymentInformation = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>($"paymentconfiguration.{_enviroment.EnvironmentName}", _distrbutedCacheExpirationConfiguration.PaymentConfiguration, ESchemaType.PaymentConfiguration);

            var config = paymentInformation.Where(x => x.FormName == formName)
                .FirstOrDefault();

            if (config == null)
            {
                throw new ApplicationException($"No payment infomation configured for {formName} form");
            }

            var paymentProvider = _paymentProviders.Where(_ => _.ProviderName == config.PaymentProvider)
                .FirstOrDefault();

            if (paymentProvider == null)
            {
                throw new ApplicationException($"No payment provider configured for provider {config.PaymentProvider}");
            }
        }

        public void CheckForInvalidQuestionOrTargetMappingValue(List<Page> pages, string formName)
        {
            var questionIds = pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.ValidatableElements)
                .Select(_ => string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping)
                .ToList();

            questionIds.ForEach(_ =>
            {
                var regex = new Regex(@"^[a-zA-Z.]+$", RegexOptions.IgnoreCase);
                if (!regex.IsMatch(_.ToString()))
                {
                    throw new ApplicationException($"The provided json '{formName}' contains invalid QuestionIDs or TargetMapping, {_.ToString()} contains invalid characters");
                }

                if (_.ToString().EndsWith(".") || _.ToString().StartsWith("."))
                {
                    throw new ApplicationException($"The provided json '{formName}' contains invalid QuestionIDs or TargetMapping, {_.ToString()} contains invalid characters");
                }
            });
        }

        public void CheckForCurrentEnvironmentSubmitSlugs(List<Page> pages, string formName)
        {
            List<Behaviour> behaviours = new List<Behaviour>();

            foreach (var page in pages)
            {
                if (page.Behaviours != null)
                {
                    foreach (var behaviour in page.Behaviours)
                    {
                        behaviours.Add(behaviour);
                    }
                }
            }

            foreach (var item in behaviours)
            {
                if (item.BehaviourType == EBehaviourType.SubmitForm || item.BehaviourType == EBehaviourType.SubmitAndPay || item.BehaviourType == EBehaviourType.SubmitPowerAutomate)
                {
                    if (item.SubmitSlugs.Count > 0)
                    {
                       var foundEnviromentSubmitSlug = false;
                        foreach (var subItem in item.SubmitSlugs)
                        {
                            if (subItem.Environment.ToLower() == _enviroment.EnvironmentName.ToS3EnvPrefix().ToLower())
                            {
                                foundEnviromentSubmitSlug = true;
                            }
                        }

                        if (!foundEnviromentSubmitSlug)
                        {
                            throw new ApplicationException($"No SubmitSlug found for {formName} form for {_enviroment.EnvironmentName}");
                        }
                    }
                }
            }
        }

        public void CheckSubmitSlugsHaveAllProperties(List<Page> pages, string formName)
        {
            List<Behaviour> behaviours = new List<Behaviour>();

            foreach (var page in pages)
            {
                if (page.Behaviours != null)
                {
                    foreach (var behaviour in page.Behaviours)
                    {
                        behaviours.Add(behaviour);
                    }
                }
            }

            foreach (var item in behaviours)
            {
                if (item.BehaviourType == EBehaviourType.SubmitForm || item.BehaviourType == EBehaviourType.SubmitAndPay || item.BehaviourType == EBehaviourType.SubmitPowerAutomate)
                {
                    if (item.SubmitSlugs.Count > 0)
                    {
                        foreach (var subItem in item.SubmitSlugs)
                        {
                            if (string.IsNullOrEmpty(subItem.URL))
                            {
                                throw new ApplicationException($"No URL found in the SubmitSlug for {formName} form");
                            }

                            if (item.BehaviourType != EBehaviourType.SubmitPowerAutomate)
                            {
                                if (string.IsNullOrEmpty(subItem.AuthToken))
                                {
                                    throw new ApplicationException($"No Auth Token found in the SubmitSlug for {formName} form");
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CheckForAcceptedFileUploadFileTypes(List<Page> pages, string formName)
        {
            var documentUploadElements = pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.ValidatableElements)
                .Where(_ => _.Type == EElementType.FileUpload)
                .Where(_ => _.Properties.AllowedFileTypes != null)
                .ToList();

            if (documentUploadElements != null)
            {
                documentUploadElements.ForEach(_ =>
                {
                    _.Properties.AllowedFileTypes.ForEach(x =>
                    {
                        if (!x.StartsWith("."))
                            throw new ApplicationException($"PageHelper::CheckForAcceptedFileUploadFileTypes, Allowed file type in FileUpload element {_.Properties.QuestionId} must have a valid extension which begins with a ., e.g. .png");
                    });
                });
            }
        }

        public void SaveFormData(string key, object value, string guid)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
            {
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            }
            if (convertedAnswers.FormData.ContainsKey(key))
            {
                convertedAnswers.FormData.Remove(key);
            }
            convertedAnswers.FormData.Add(key, value);
            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));

        }
    }
}