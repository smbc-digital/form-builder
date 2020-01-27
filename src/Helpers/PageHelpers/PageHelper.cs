using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        void hasDuplicateQuestionIDs(List<Page> pages, string formName);
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressSearchResults = null, List<OrganisationSearchResult> organisationSearchResults = null);
        void SaveAnswers(Dictionary<string, string> viewModel, string guid, string form);
        Task<ProcessRequestEntity> ProcessOrganisationJourney(string journey, Page currentPage, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<OrganisationSearchResult> organisationResults);
        Task<ProcessRequestEntity> ProcessStreetJourney(string journey, Page currentPage, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults);
        Task<ProcessRequestEntity> ProcessAddressJourney(string journey, Page currentPage, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults);
        void CheckForInvalidQuestionOrTargetMappingValue(List<Page> pages, string formName);
        void CheckForPaymentConfiguration(List<Page> pages, string formName);
    }

    public class PageHelper : IPageHelper
    {
        private readonly IViewRender _viewRender;
        private readonly IElementHelper _elementHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DisallowedAnswerKeysConfiguration _disallowedKeys;
        private readonly IHostingEnvironment _enviroment;
        private readonly PaymentInformationConfiguration _paymentInformationConfiguration;
        public PageHelper(IViewRender viewRender, IElementHelper elementHelper, IDistributedCacheWrapper distributedCache, IOptions<DisallowedAnswerKeysConfiguration> disallowedKeys, IOptions<PaymentInformationConfiguration> paymentInformationConfiguration, IHostingEnvironment enviroment)
        {
            _viewRender = viewRender;
            _elementHelper = elementHelper;
            _distributedCache = distributedCache;
            _disallowedKeys = disallowedKeys.Value;
            _enviroment = enviroment;
            _paymentInformationConfiguration = paymentInformationConfiguration.Value;
        }

        public async Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressAndStreetSearchResults = null, List<OrganisationSearchResult> organisationSearchResults = null)
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

        public void SaveAnswers(Dictionary<string, string> viewModel, string guid, string form)
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

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public async Task<ProcessRequestEntity> ProcessStreetJourney(string journey, Page currentPage, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults)
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
        
        public async Task<ProcessRequestEntity> ProcessAddressJourney(string journey, Page currentPage, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults)
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
                    throw new ApplicationException($"PageHelper.ProcessAddressJourney: Unknown journey type");
            }
        }

        public async Task<ProcessRequestEntity> ProcessOrganisationJourney(string journey, Page currentPage, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<OrganisationSearchResult> organisationResults)
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

        public void hasDuplicateQuestionIDs(List<Page> pages, string formName)
        {
            List<string> qIds = new List<string>();
            foreach (var page in pages)
            {
                foreach (var element in page.Elements)
                {
                    if (
                        element.Type == EElementType.Address
                        || element.Type == EElementType.Textbox
                        || element.Type == EElementType.Textarea
                        || element.Type == EElementType.Select
                        || element.Type == EElementType.Radio
                        || element.Type == EElementType.Street
                        || element.Type == EElementType.Checkbox
                        || element.Type == EElementType.DateInput
                        || element.Type == EElementType.TimeInput
                        || element.Type == EElementType.Organisation
                        )
                    {
                        qIds.Add(element.Properties.QuestionId);
                    }
                }
            }

            var hashSet = new HashSet<string>();
            foreach(var id in qIds)
            {
                if (!hashSet.Add(id))
                {
                    throw new ApplicationException($"The provided json '{formName}' has duplicate QuestionIDs");
                }
            }
        }

        public void CheckForPaymentConfiguration(List<Page> pages, string formName)
        {
            var containsPayment = pages.Where(x => x.Behaviours != null)
                .SelectMany(x => x.Behaviours)
                .Any(x => x.BehaviourType == EBehaviourType.SubmitAndPay);

                if(!containsPayment)
                    return;
            
            var config = _paymentInformationConfiguration.PaymentConfigs.Where(x => x.FormName == formName)
                .FirstOrDefault();

            if(config == null){
                throw new ApplicationException($"No payment infomation configured for {formName} form");
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
    }
}