using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.Session;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.StorageProvider;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Helpers.PageHelpers
{
    public class PageHelper : IPageHelper
    {
        private readonly IViewRender _viewRender;
        private readonly IElementHelper _elementHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly FormConfiguration _disallowedKeys;
        private readonly IWebHostEnvironment _environment;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly ICache _cache;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly ISessionHelper _sessionHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PageHelper(IViewRender viewRender, IElementHelper elementHelper, IDistributedCacheWrapper distributedCache,
            IOptions<FormConfiguration> disallowedKeys, IWebHostEnvironment enviroment, ICache cache,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            IEnumerable<IPaymentProvider> paymentProviders, ISessionHelper sessionHelper, IHttpContextAccessor httpContextAccessor)
        {
            _viewRender = viewRender;
            _elementHelper = elementHelper;
            _distributedCache = distributedCache;
            _disallowedKeys = disallowedKeys.Value;
            _environment = enviroment;
            _cache = cache;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _paymentProviders = paymentProviders;
            _sessionHelper = sessionHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FormBuilderViewModel> GenerateHtml(
            Page page,
            Dictionary<string, dynamic> viewModel,
            FormSchema baseForm,
            string guid,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            var formModel = new FormBuilderViewModel();

            if (page.PageSlug.ToLower() != "success" && !page.HideTitle)
                formModel.RawHTML += await _viewRender.RenderAsync("H1", new Element { Properties = new BaseProperty { Text = page.GetPageTitle() } });

            foreach (var element in page.Elements)
            {
                string html = await element.RenderAsync(
                    _viewRender,
                    _elementHelper,
                    guid,
                    viewModel,
                    page,
                    baseForm,
                    _environment,
                    formAnswers,
                    results
                    );
                if (element.Properties.isConditionalElement)
                {
                    formModel.RawHTML = formModel.RawHTML.Replace(SystemConstants.ConditionalElementReplacementString + element.Properties.QuestionId, html);
                }
                else
                {
                    formModel.RawHTML += html;
                }

            }
            return formModel;
        }

        public void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IEnumerable<CustomFormFile> files, bool isPageValid, bool appendMultipleFileUploadParts = false)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };
            var currentPageAnswers = new PageAnswers();

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            if (convertedAnswers.Pages != null && convertedAnswers.Pages.Any(_ => _.PageSlug == viewModel["Path"].ToLower()))
            {
                currentPageAnswers = convertedAnswers.Pages.Where(_ => _.PageSlug == viewModel["Path"].ToLower()).ToList().FirstOrDefault();
                convertedAnswers.Pages = convertedAnswers.Pages.Where(_ => _.PageSlug != viewModel["Path"].ToLower()).ToList();
            }

            var answers = new List<Answers>();

            foreach (var item in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Any(key => item.Key.Contains(key)))
                    answers.Add(new Answers { QuestionId = item.Key, Response = item.Value });
            }

            if ((files == null || !files.Any()) && currentPageAnswers.Answers != null && currentPageAnswers.Answers.Any() && isPageValid && appendMultipleFileUploadParts)
                answers = currentPageAnswers.Answers;

            if (files != null && files.Any() && isPageValid)
                answers = SaveFormFileAnswers(answers, files, appendMultipleFileUploadParts, currentPageAnswers);

            convertedAnswers.Pages?.Add(new PageAnswers
            {
                PageSlug = viewModel["Path"].ToLower(),
                Answers = answers
            });

            convertedAnswers.Path = viewModel["Path"];
            convertedAnswers.FormName = form;

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public void SaveCaseReference(string guid, string caseReference)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            convertedAnswers.CaseReference = caseReference;
            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
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
                        && element.Type != EElementType.UploadedFilesSummary
                        && element.Type != EElementType.Warning
                        && element.Type != EElementType.Link
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

        public void CheckQuestionIdExistsForBookingCustomerAddressId(List<Page> pages, string formName)
        {
            var bookingElements = pages.SelectMany(_ => _.Elements)
                .Where(_ => _.Type.Equals(EElementType.Booking)).ToList();

            if (bookingElements.Count > 0)
            {
                foreach (var bookingElement in bookingElements)
                {
                    if (!string.IsNullOrEmpty(bookingElement.Properties.CustomerAddressId))
                    {
                        var matchingElement = pages.SelectMany(_ => _.Elements)
                            .FirstOrDefault(_ =>
                                _.Properties.QuestionId != null &&
                                _.Properties.QuestionId.Contains(bookingElement.Properties.CustomerAddressId));

                        if (matchingElement == null)
                            throw new ApplicationException($"The provided json '{formName}' does not contain an element with questionId of " +
                                                           $"'{bookingElement.Properties.CustomerAddressId}' for booking element " +
                                                           $"'{bookingElement.Properties.QuestionId}'");
                    }
                }
                
            }
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

        public void CheckConditionalElementsAreValid(List<Page> pages, string formName)
        {
            var radioWithConditionals = pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.ValidatableElements)
                .Where(_ => _.Type == EElementType.Radio)
                .Where(_ => _.Properties.Options.Any(_ => _.HasConditionalElement))
                .ToList();

            var conditionalElements = pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.Elements)
                .Where(_ => _.Properties.isConditionalElement)
                .ToList();

            foreach (var radio in radioWithConditionals)
            {
                foreach (var option in radio.Properties.Options)
                {
                    if (
                        option.HasConditionalElement &&
                        !string.IsNullOrEmpty(option.ConditionalElementId) &&
                        !conditionalElements.Any(_ => _.Properties.QuestionId == option.ConditionalElementId))
                        throw new ApplicationException($"The provided json '{formName}' does not contain a conditional element for the '{option.Value}' value of radio '{radio.Properties.QuestionId}'");

                    if (
                        option.HasConditionalElement && 
                        !string.IsNullOrEmpty(option.ConditionalElementId) && 
                        !pages.Any(page => page.ValidatableElements.Contains(radio) && page.Elements.Any(_ => _.Properties.QuestionId == option.ConditionalElementId && _.Properties.isConditionalElement)))
                        throw new ApplicationException($"The provided json '{formName}' contains the conditional element for the '{option.Value}' value of radio '{radio.Properties.QuestionId}' on a different page to the radio element");

                    
                    conditionalElements.Remove(conditionalElements.FirstOrDefault(_ => _.Properties.QuestionId == option.ConditionalElementId));
                }
            }

            if (conditionalElements.Count > 0)
                throw new ApplicationException($"The provided json '{formName}' has conditional elements '{String.Join(", ", conditionalElements.Select(_ => _.Properties.QuestionId))}' not assigned to radio options");

        }

        public void SaveFormData(string key, object value, string guid, string formName)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            if (convertedAnswers.FormData.ContainsKey(key))
                convertedAnswers.FormData.Remove(key);

            convertedAnswers.FormData.Add(key, value);
            convertedAnswers.FormName = formName;

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public void SaveNonQuestionAnswers(Dictionary<string, object> values, string form, string path, string guid)
        {
            if (!values.Any())
                return;

            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            convertedAnswers.FormName = form;
            convertedAnswers.Path = path;

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            values.ToList().ForEach((_) =>
            {
                if (convertedAnswers.AdditionalFormData.ContainsKey(_.Key))
                    convertedAnswers.AdditionalFormData.Remove(_.Key);

                convertedAnswers.AdditionalFormData.Add(_.Key, _.Value);
            });

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }


        public void CheckForDocumentDownload(FormSchema formSchema)
        {
            if (!formSchema.DocumentDownload) return;

            if (formSchema.DocumentType.Any())
            {
                if (formSchema.DocumentType.Any(_ => _ == EDocumentType.Unknown))
                    throw new ApplicationException("PageHelper::CheckForDocumentDownload, Unknown document download type configured");
            }
            else
            {
                throw new ApplicationException("PageHelper::CheckForDocumentDownload, No document download type configured");
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
                            if (_.HttpActionType.Equals(EHttpActionType.Unknown))
                                throw new Exception("PageHelper::CheckForIncomingFormDataValues, EHttpActionType cannot be unknwon, set to Get or Post");

                            if (string.IsNullOrEmpty(_.QuestionId) || string.IsNullOrEmpty(_.Name))
                                throw new Exception("PageHelper::CheckForIncomingFormDataValues, QuestionId or Name cannot be empty");
                        }
                    ));
            }
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

            var validateActions = formSchema.FormActions.Where(_ => _.Type.Equals(EActionType.Validate))
               .Concat(formSchema.Pages.SelectMany(_ => _.PageActions)
               .Where(_ => _.Type == EActionType.Validate)).ToList();

            CheckEmailAction(userEmail);
            CheckEmailAction(backOfficeEmail);
            CheckRetrieveExternalDataAction(retrieveExternalDataActions);
            CheckValidateAction(validateActions);
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

        private void CheckValidateAction(List<IAction> actions)
        {
            if (!actions.Any())
                return;

            actions.ForEach(action =>
            {
                var foundSlug = action.Properties.PageActionSlugs.FirstOrDefault(_ => _.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower()));

                if (foundSlug == null)
                    throw new ApplicationException($"PageHelper:CheckValidateAction, Validate there is no PageActionSlug for {_environment.EnvironmentName}");

                if (string.IsNullOrEmpty(foundSlug.URL))
                    throw new ApplicationException("PageHelper:CheckValidateAction, Validate action type does not contain a url");

                if (action.Properties.HttpActionType == EHttpActionType.Unknown)
                    throw new ApplicationException("PageHelper:CheckValidateAction, Validate action type does not contain 'Unknown'");
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
                    throw new ApplicationException("PageHelper:CheckRetrieveExternalDataAction, RetrieveExternalDataAction action type does not contain a url");

                if (string.IsNullOrEmpty(action.Properties.TargetQuestionId))
                    throw new ApplicationException("PageHelper:CheckRetrieveExternalDataAction, RetrieveExternalDataAction action type does not contain a TargetQuestionId");
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
                : new FormAnswers { Pages = new List<PageAnswers>() };

            var answers = convertedAnswers.Pages.SelectMany(_ => _.Answers).ToDictionary(_ => _.QuestionId, _ => _.Response);

            return pages.FirstOrDefault(page => page.CheckPageMeetsConditions(answers));
        }

        public void CheckAddressNoManualTextIsSet(List<Page> pages)
        {
            var addressElements = pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.Elements)
                .Where(_ => _.Type == EElementType.Address)
                .Where(_ => _.Properties.DisableManualAddress)
                .ToList();

            addressElements.ForEach(element =>
            {
                if (string.IsNullOrWhiteSpace(element.Properties.NoManualAddressDetailText))
                    throw new ApplicationException("AddressElement:DisableManualAddress set to true, NoManualAddressDetailText must have value");
            });
        }

        public void CheckForAnyConditionType(List<Page> pages)
        {
            var anyConditionType = new List<Condition>();

            var anyConditionTypeRenderConditions = pages.Where(_ => _.Behaviours != null)
                .SelectMany(_ => _.Behaviours)
                .Where(_ => _.Conditions != null)
                .SelectMany(_ => _.Conditions)
                .Where(_ => _.ConditionType == ECondition.Any)
                .ToList();

            var anyConditionTypeBehaviours = pages.Where(_ => _.RenderConditions != null)
                .SelectMany(_ => _.RenderConditions)
                .Where(_ => _.ConditionType == ECondition.Any)
                .ToList();

            anyConditionType.AddRange(anyConditionTypeRenderConditions);
            anyConditionType.AddRange(anyConditionTypeBehaviours);

            if (anyConditionType.Any())
            {
                anyConditionType.ForEach(condition =>
                {
                    if (string.IsNullOrEmpty(condition.ComparisonValue))
                        throw new ApplicationException("PageHelper:CheckForAnyConditionType, any condition type requires a comparison value");
                });
            }

        }

        public List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files, bool isMultipleFileUploadElementType, PageAnswers currentAnswersForFileUpload)
        {
            files.GroupBy(_ => _.QuestionId).ToList().ForEach(file =>
            {
                var fileUploadModel = new List<FileUploadModel>();
                var filsToAdd = file.ToList();
                if (isMultipleFileUploadElementType)
                {
                    var data = currentAnswersForFileUpload.Answers?.FirstOrDefault(_ => _.QuestionId.Equals(file.Key))?.Response;
                    if (data != null)
                    {
                        List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(data.ToString());
                        fileUploadModel.AddRange(response);
                        filsToAdd = filsToAdd.Where(_ => !response.Any(x => WebUtility.HtmlEncode(_.UntrustedOriginalFileName) == x.TrustedOriginalFileName)).ToList();
                    }
                }

                var keys = filsToAdd.Select(_ => $"file-{file.Key}-{Guid.NewGuid()}").ToArray();

                var fileContent = filsToAdd.Select(_ => _.Base64EncodedContent).ToList();

                for (int i = 0; i < fileContent.Count; i++)
                {
                    _distributedCache.SetStringAsync(keys[i], JsonConvert.SerializeObject(fileContent[i]), _distributedCacheExpirationConfiguration.FileUpload);
                }

                fileUploadModel.AddRange(filsToAdd.Select((_, index) => new FileUploadModel
                {
                    Key = keys[index],
                    TrustedOriginalFileName = WebUtility.HtmlEncode(_.UntrustedOriginalFileName),
                    UntrustedOriginalFileName = _.UntrustedOriginalFileName,
                    FileSize = _.Length
                }).ToList());

                if (answers.Exists(_ => _.QuestionId == file.Key))
                {
                    var fileUploadAnswer = answers.FirstOrDefault(_ => _.QuestionId == file.Key);
                    if (fileUploadAnswer != null)
                        fileUploadAnswer.Response = fileUploadModel;
                }
                else
                {
                    answers.Add(new Answers { QuestionId = file.Key, Response = fileUploadModel });
                }
            });

            return answers;
        }

        public void CheckUploadedFilesSummaryQuestionsIsSet(List<Page> pages)
        {
            var fileSummaryElements = pages.SelectMany(_ => _.Elements)
                .Where(_ => _.Type.Equals(EElementType.UploadedFilesSummary))
                .ToList();

            if (fileSummaryElements.Any())
            {
                fileSummaryElements.ForEach((element) =>
                {
                    if (string.IsNullOrEmpty(element.Properties.Text))
                        throw new ApplicationException("PageHelper:CheckUploadedFilesSummaryQuestionsIsSet, Uploaded files summary text must not be empty.");

                    if (!element.Properties.FileUploadQuestionIds.Any())
                        throw new ApplicationException("PageHelper:CheckUploadedFilesSummaryQuestionsIsSet, Uploaded files summary must have atleast one file questionId specified to display the list of uploaded files.");
                });
            }
        }

        public void CheckForBookingElement(List<Page> pages)
        {
            var bookingElements = pages.SelectMany(_ => _.ValidatableElements)
                .Where(_ => _.Type.Equals(EElementType.Booking))
                .ToList();

            if (bookingElements.Any())
            {
                bookingElements.ForEach((booking) =>
                {
                    if (string.IsNullOrEmpty(booking.Properties.BookingProvider))
                        throw new ApplicationException("PageHelper:CheckForBookingElement, Booking element requires a valid booking provider property.");

                    if (booking.Properties.AppointmentType == Guid.Empty)
                        throw new ApplicationException("PageHelper:CheckForBookingElement, Booking element requires a AppointmentType property.");

                    if (booking.Properties.OptionalResources.Any())
                    {
                        booking.Properties.OptionalResources.ForEach(resource =>
                        {
                            if (resource.Quantity <= 0)
                                throw new ApplicationException("PageHelper:CheckForBookingElement, Booking element optional resources are invalid, cannot have a quantity less than 0");

                            if (resource.ResourceId.Equals(Guid.Empty))
                                throw new ApplicationException("PageHelper:CheckForBookingElement, Booking element optional resources are invalid, ResourceId cannot be an empty Guid.");
                        });
                    }
                });

                if (!pages.Any(_ => _.PageSlug.ToLower().Equals(BookingConstants.NO_APPOINTMENT_AVAILABLE)))
                    throw new ApplicationException($"PageHelper:CheckForBookingElement, Form contains booking element but is missing required page with slug {BookingConstants.NO_APPOINTMENT_AVAILABLE}.");

                var additionalRequiredElements = pages.SelectMany(_ => _.ValidatableElements)
                    .Where(_ => _.Properties != null && _.Properties.TargetMapping != null)
                    .Where(_ => _.Properties.TargetMapping.ToLower().Equals("customer.firstname")
                        || _.Properties.TargetMapping.ToLower().Equals("customer.lastname"))
                    .ToList();

                if (additionalRequiredElements.Count != 2)
                    throw new ApplicationException("PageHelper:CheckForBookingElement, Booking element requires customer firstname/lastname elements for reservation");
            }
        }
    }
}