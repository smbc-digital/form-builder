using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.FileUploadService;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Dynamic;
using form_builder.Helpers.Session;
using Microsoft.Extensions.Hosting;

namespace form_builder.Helpers.PageHelpers
{
    public class PageHelper : IPageHelper
    {
        private readonly IViewRender _viewRender;
        private readonly IElementHelper _elementHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DisallowedAnswerKeysConfiguration _disallowedKeys;
        private readonly IWebHostEnvironment _environment;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly ICache _cache;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly IFileUploadService _fileUploadService;
        private readonly ISessionHelper _sessionHelper;

        public PageHelper(IViewRender viewRender, IElementHelper elementHelper, IDistributedCacheWrapper distributedCache,
            IOptions<DisallowedAnswerKeysConfiguration> disallowedKeys, IWebHostEnvironment enviroment, ICache cache,
            IOptions<DistributedCacheExpirationConfiguration> distrbutedCacheExpirationConfiguration,
            IEnumerable<IPaymentProvider> paymentProviders, IFileUploadService fileUploadService, ISessionHelper sessionHelper)
        {
            _viewRender = viewRender;
            _elementHelper = elementHelper;
            _distributedCache = distributedCache;
            _disallowedKeys = disallowedKeys.Value;
            _environment = enviroment;
            _cache = cache;
            _distributedCacheExpirationConfiguration = distrbutedCacheExpirationConfiguration.Value;
            _paymentProviders = paymentProviders;
            _fileUploadService = fileUploadService;
            _sessionHelper = sessionHelper;
        }

        public async Task<FormBuilderViewModel> GenerateHtml(
            Page page,
            Dictionary<string, dynamic> viewModel,
            FormSchema baseForm,
            string guid,
            List<object> results = null)
        {
            var formModel = new FormBuilderViewModel();

            if (page.PageSlug.ToLower() != "success" && !page.HideTitle)
                formModel.RawHTML += await _viewRender.RenderAsync("H1", new Element { Properties = new BaseProperty { Text = page.GetPageTitle() } });

            foreach (var element in page.Elements)
                formModel.RawHTML += await element.RenderAsync(
                    _viewRender,
                    _elementHelper,
                    guid,
                    viewModel,
                    page,
                    baseForm,
                    _environment,
                    results);

            return formModel;
        }

        public void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IEnumerable<CustomFormFile> files, bool isPageValid)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            if (convertedAnswers.Pages != null && convertedAnswers.Pages.Any(_ => _.PageSlug == viewModel["Path"].ToLower()))
                convertedAnswers.Pages = convertedAnswers.Pages.Where(_ => _.PageSlug != viewModel["Path"].ToLower()).ToList();

            var answers = new List<Answers>();

            foreach (var item in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Any(key => item.Key.Contains(key)))
                    answers.Add(new Answers { QuestionId = item.Key, Response = item.Value });
            }

            if (files != null && files.Any() && isPageValid)
                answers = _fileUploadService.SaveFormFileAnswers(answers, files);

            convertedAnswers.Pages?.Add(new PageAnswers
            {
                PageSlug = viewModel["Path"].ToLower(),
                Answers = answers
            });

            convertedAnswers.Path = viewModel["Path"];
            convertedAnswers.FormName = form;

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers), CancellationToken.None);
        }

        public void HasDuplicateQuestionIDs(List<Page> pages, string formName)
        {
            var questionIds = new List<string>();
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
                        && element.Type != EElementType.HR
                        )
                    {
                        questionIds.Add(element.Properties.QuestionId);
                    }
                }
            }

            var hashSet = new HashSet<string>();
            if (questionIds.Any(id => !hashSet.Add(id)))
                throw new ApplicationException($"The provided json '{formName}' has duplicate QuestionIDs");
        }

        public void CheckForEmptyBehaviourSlugs(List<Page> pages, string formName)
        {
            var behaviours = new List<Behaviour>();

            foreach (var page in pages.Where(page => page.Behaviours != null))
            {
                behaviours.AddRange(page.Behaviours);
            }

            if (behaviours.Any(item => string.IsNullOrEmpty(item.PageSlug) && (item.SubmitSlugs == null || item.SubmitSlugs.Count == 0)))
                throw new ApplicationException($"Incorrectly configured behaviour slug was discovered in {formName} form");
        }

        public async Task CheckForPaymentConfiguration(List<Page> pages, string formName)
        {
            var containsPayment = pages.Where(x => x.Behaviours != null)
                .SelectMany(x => x.Behaviours)
                .Any(x => x.BehaviourType == EBehaviourType.SubmitAndPay);

            if (!containsPayment)
                return;

            var paymentInformation = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>($"paymentconfiguration.{_environment.EnvironmentName}", _distributedCacheExpirationConfiguration.PaymentConfiguration, ESchemaType.PaymentConfiguration);

            var config = paymentInformation.FirstOrDefault(x => x.FormName == formName);

            if (config == null)
                throw new ApplicationException($"No payment information configured for {formName} form");

            var paymentProvider = _paymentProviders.FirstOrDefault(_ => _.ProviderName == config.PaymentProvider);

            if (paymentProvider == null)
                throw new ApplicationException($"No payment provider configured for provider {config.PaymentProvider}");

            if (config.Settings.ComplexCalculationRequired)
            {
                var paymentSummaryElement = pages.SelectMany(_ => _.Elements)
                    .First(_ => _.Type == EElementType.PaymentSummary);

                if (!_environment.IsEnvironment("local") && !paymentSummaryElement.Properties.CalculationSlugs.Where(_ => !_.Environment.ToLower().Equals("local")).Any(_ => _.URL.StartsWith("https://")))
                    throw new ApplicationException("PaymentSummary::CalculateCostUrl must start with https");
            }
        }

        public void CheckForInvalidQuestionOrTargetMappingValue(List<Page> pages, string formName)
        {
            var questionIds = pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.ValidatableElements)
                .Select(_ => string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping)
                .ToList();

            questionIds.ForEach(questionId =>
            {
                var regex = new Regex(@"^[a-zA-Z.]+$", RegexOptions.IgnoreCase);
                if (!regex.IsMatch(questionId.ToString()))
                    throw new ApplicationException($"The provided json '{formName}' contains invalid QuestionIDs or TargetMapping, {questionId} contains invalid characters");

                if (questionId.ToString().EndsWith(".") || questionId.ToString().StartsWith("."))
                    throw new ApplicationException($"The provided json '{formName}' contains invalid QuestionIDs or TargetMapping, {questionId} contains invalid characters");
            });
        }

        public void CheckForCurrentEnvironmentSubmitSlugs(List<Page> pages, string formName)
        {
            var behaviours = pages.Where(page => page.Behaviours != null).SelectMany(page => page.Behaviours).ToList();

            foreach (var item in behaviours)
            {
                if (item.BehaviourType != EBehaviourType.SubmitForm && item.BehaviourType != EBehaviourType.SubmitAndPay) continue;
                if (item.SubmitSlugs.Count <= 0) continue;

                var foundEnvironmentSubmitSlug = false;
                foreach (var subItem in item.SubmitSlugs.Where(subItem => subItem.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower())))
                {
                    foundEnvironmentSubmitSlug = true;
                }

                if (!foundEnvironmentSubmitSlug)
                    throw new ApplicationException($"No SubmitSlug found for {formName} form for {_environment.EnvironmentName}");
            }
        }

        public void CheckSubmitSlugsHaveAllProperties(List<Page> pages, string formName)
        {
            var behaviours = pages.Where(page => page.Behaviours != null).SelectMany(page => page.Behaviours).ToList();

            foreach (var item in behaviours)
            {
                if (item.BehaviourType != EBehaviourType.SubmitForm && item.BehaviourType != EBehaviourType.SubmitAndPay) continue;

                if (item.SubmitSlugs.Count <= 0) continue;

                foreach (var subItem in item.SubmitSlugs)
                {
                    if (string.IsNullOrEmpty(subItem.URL))
                        throw new ApplicationException($"No URL found in the SubmitSlug for {formName} form");

                    if (string.IsNullOrEmpty(subItem.AuthToken))
                        throw new ApplicationException($"No Auth Token found in the SubmitSlug for {formName} form");

                    if (!_environment.IsEnvironment("local") && !subItem.Environment.ToLower().Equals("local") && !subItem.URL.StartsWith("https://"))
                        throw new Exception("SubmitUrl must start with https");
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

            documentUploadElements.ForEach(_ =>
            {
                _.Properties.AllowedFileTypes.ForEach(x =>
                {
                    if (!x.StartsWith("."))
                        throw new ApplicationException($"PageHelper::CheckForAcceptedFileUploadFileTypes, Allowed file type in FileUpload element {_.Properties.QuestionId} must have a valid extension which begins with a ., e.g. .png");
                });
            });
        }

        public void SaveFormData(string key, object value, string guid)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            if (convertedAnswers.FormData.ContainsKey(key))
                convertedAnswers.FormData.Remove(key);

            convertedAnswers.FormData.Add(key, value);
            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public void CheckForDocumentDownload(FormSchema formSchema)
        {
            if (!formSchema.DocumentDownload) return;

            if (formSchema.DocumentType.Any())
            {
                if (formSchema.DocumentType.Any(_ => _ == EDocumentType.Unknown))
                    throw new ApplicationException($"PageHelper::CheckForDocumentDownload, Unknown document download type configured");
            }
            else
            {
                throw new ApplicationException($"PageHelper::CheckForDocumentDownload, No document download type configured");
            }
        }

        public void CheckForIncomingFormDataValues(List<Page> pages)
        {
            if (pages.Any(_ => _.HasIncomingValues))
            {
                pages.Where(_ => _.HasIncomingValues)
                    .ToList()
                    .ForEach(x => x.IncomingValues.ForEach(_ =>
                        {
                            if (string.IsNullOrEmpty(_.QuestionId) || string.IsNullOrEmpty(_.Name))
                                throw new Exception("PageHelper::CheckForIncomingFormDataValues, QuestionId or Name cannot be empty");
                        }
                    ));
            }
        }

        public Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, Dictionary<string, dynamic> formData)
        {
            page.IncomingValues.ForEach(_ =>
            {
                var containsValue = formData.ContainsKey(_.Name);

                if (!_.Optional && !containsValue)
                    throw new Exception($"DictionaryExtensions::IncomingValue, FormData does not contains {_.Name} required value");

                if (!containsValue) return;

                formData = RecursiveCheckAndCreate(_.QuestionId, formData[_.Name], formData);
                formData.Remove(_.Name);
            });

            return formData;
        }

        private IDictionary<string, dynamic> RecursiveCheckAndCreate(string targetMapping, string value, IDictionary<string, dynamic> obj)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length == 1)
            {
                obj.Add(splitTargets[0], value);
                return obj;
            }

            object subObject;
            if (!obj.TryGetValue(splitTargets[0], out subObject))
                subObject = new ExpandoObject();

            subObject = RecursiveCheckAndCreate(targetMapping.Replace($"{splitTargets[0]}.", string.Empty), value, subObject as IDictionary<string, dynamic>);

            obj.Remove(splitTargets[0]);
            obj.Add(splitTargets[0], subObject);

            return obj;
        }

        public void CheckForPageActions(FormSchema formSchema)
        {
            var userEmail = formSchema.FormActions.Where(_ => _.Type.Equals(EActionType.UserEmail))
                .Concat(formSchema.Pages.SelectMany(_ => _.PageActions)
                .Where(_ => _.Type == EActionType.UserEmail)).ToList();

            var backOfficeEmail = formSchema.FormActions.Where(_ => _.Type.Equals(EActionType.BackOfficeEmail))
                .Concat(formSchema.Pages.SelectMany(_ => _.PageActions)
                .Where(_ => _.Type == EActionType.BackOfficeEmail)).ToList();

            var retrieveExternalDataActions = formSchema.FormActions.Where(_ => _.Type.Equals(EActionType.RetrieveExternalData))
                .Concat(formSchema.Pages.SelectMany(_ => _.PageActions)
                .Where(_ => _.Type == EActionType.RetrieveExternalData)).ToList();

            CheckEmailAction(userEmail);
            CheckEmailAction(backOfficeEmail);
            CheckRetrieveExternalDataAction(retrieveExternalDataActions);
        }

        private void CheckEmailAction(List<IAction> actions)
        {
            if (!actions.Any())
                return;

            actions.ForEach(action =>
            {
                if (string.IsNullOrEmpty(action.Properties.Content))
                    throw new ApplicationException("PageHelper:: CheckEmailAction, Content doesn't have a value");

                if (string.IsNullOrEmpty(action.Properties.To))
                    throw new ApplicationException("PageHelper:: CheckEmailAction, To doesn't have a value");

                if (string.IsNullOrEmpty(action.Properties.From))
                    throw new ApplicationException("PageHelper:: CheckEmailAction, From doesn't have a value");

                if (string.IsNullOrEmpty(action.Properties.Subject))
                    throw new ApplicationException("PageHelper:: CheckEmailAction, Subject doesn't have a value");
            });
        }

        private void CheckRetrieveExternalDataAction(List<IAction> actions)
        {
            if (!actions.Any())
                return;

            actions.ForEach(action =>
            {
                var foundSlug = action.Properties.PageActionSlugs.FirstOrDefault(_ => _.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower()));

                if (foundSlug == null)
                    throw new ApplicationException($"PageHelper:CheckRetrieveExternalDataAction, RetrieveExternalDataAction there is no PageActionSlug for {_environment.EnvironmentName}");

                if (string.IsNullOrEmpty(foundSlug.URL))
                    throw new ApplicationException($"PageHelper:CheckRetrieveExternalDataAction, RetrieveExternalDataAction action type does not contain a url");

                if (string.IsNullOrEmpty(action.Properties.TargetQuestionId))
                    throw new ApplicationException($"PageHelper:CheckRetrieveExternalDataAction, RetrieveExternalDataAction action type does not contain a TargetQuestionId");
            });
        }

        public void CheckRenderConditionsValid(List<Page> pages)
        {
            var groups = pages.GroupBy(_ => _.PageSlug, (key, g) => new { Slug = key, Pages = g.ToList() });

            foreach (var group in groups)
            {
                if (group.Pages.Count(_ => !_.HasRenderConditions) > 1)
                    throw new ApplicationException($"PageHelper:CheckRenderConditionsValid, More than one {@group.Slug} page has no render conditions");
            }
        }

        public Page GetPageWithMatchingRenderConditions(List<Page> pages)
        {
            var guid = _sessionHelper.GetSessionGuid();
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = !string.IsNullOrEmpty(formData)
                ? JsonConvert.DeserializeObject<FormAnswers>(formData)
                : new FormAnswers {Pages = new List<PageAnswers>()};

            var answers = convertedAnswers.Pages.SelectMany(_ => _.Answers).ToDictionary(_ => _.QuestionId, _ => _.Response);

            return pages.FirstOrDefault(page => page.CheckPageMeetsConditions(answers));
        }
    }
}