using form_builder.Configuration;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Models;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Submit;
using form_builder.Services.MappingService.Entities;
using form_builder.SubmissionActions;
using form_builder.TagParsers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.SubmitService
{
    public class SubmitService : ISubmitService
    {
        private readonly IGateway _gateway;
        private readonly IPageHelper _pageHelper;
        private readonly IWebHostEnvironment _environment;
        private readonly SubmissionServiceConfiguration _submissionServiceConfiguration;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly ISchemaFactory _schemaFactory;
        private readonly IReferenceNumberProvider _referenceNumberProvider;
        private readonly IEnumerable<ISubmitProvider> _submitProviders;
        private readonly IPaymentHelper _paymentHelper;
        private readonly IPostSubmissionAction _postSubmissionAction;
        private readonly IEnumerable<ITagParser> _tagParsers;
        private readonly ILogger<SubmitService> _logger;

        public SubmitService(
            IGateway gateway,
            IPageHelper pageHelper,
            IWebHostEnvironment environment,
            IOptions<SubmissionServiceConfiguration> submissionServiceConfiguration,
            IDistributedCacheWrapper distributedCache,
            ISchemaFactory schemaFactory,
            IReferenceNumberProvider referenceNumberProvider,
            IEnumerable<ISubmitProvider> submitProviders,
            IPaymentHelper paymentHelper,
            IPostSubmissionAction postSubmissionAction,
            IEnumerable<ITagParser> tagParsers,
            ILogger<SubmitService> logger
            )
        {
            _gateway = gateway;
            _pageHelper = pageHelper;
            _environment = environment;
            _submissionServiceConfiguration = submissionServiceConfiguration.Value;
            _distributedCache = distributedCache;
            _schemaFactory = schemaFactory;
            _referenceNumberProvider = referenceNumberProvider;
            _submitProviders = submitProviders;
            _paymentHelper = paymentHelper;
            _postSubmissionAction = postSubmissionAction;
            _tagParsers = tagParsers;
            _logger = logger;
        }

        public async Task PreProcessSubmission(string form, string sessionGuid)
        {
            var baseForm = await _schemaFactory.Build(form);
            if (baseForm.GenerateReferenceNumber)
                _pageHelper.SaveCaseReference(sessionGuid, _referenceNumberProvider.GetReference(baseForm.ReferencePrefix), true, baseForm.GeneratedReferenceNumberMapping);

            if (baseForm.SavePaymentAmount)
                _pageHelper.SavePaymentAmount(sessionGuid, _paymentHelper.GetFormPaymentInformation(baseForm.BaseURL).Result.Settings.Amount, baseForm.PaymentAmountMapping);
        }

        public async Task<string> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            var reference = string.Empty;

            if (mappingEntity.BaseForm.GenerateReferenceNumber)
            {
                var answers = JsonConvert.DeserializeObject<FormAnswers>(_distributedCache.GetString(sessionGuid));
                reference = answers.CaseReference;
            }

            if (mappingEntity.BaseForm.Pages is not null && mappingEntity.FormAnswers.Pages is not null)
                await _postSubmissionAction.ConfirmResult(mappingEntity, _environment.EnvironmentName);

            _logger.LogInformation($"SubmitService.ProcessSubmission:{sessionGuid} Submitting {form}");
            var submissionReference = _submissionServiceConfiguration.FakeSubmission
               ? ProcessFakeSubmission(mappingEntity, form, sessionGuid, reference)
               : await ProcessGenuineSubmission(mappingEntity, form, sessionGuid, reference);
            
            _logger.LogInformation($"SubmitService.ProcessSubmission:{sessionGuid} Submitted successfully {form} - {submissionReference}");
            return submissionReference;
        }

        private string ProcessFakeSubmission(MappingEntity mappingEntity, string form, string sessionGuid, string reference)
        {
            if (!string.IsNullOrEmpty(reference))
                return reference;

            _pageHelper.SaveCaseReference(sessionGuid, "123456");
            return "123456";
        }

        private async Task<string> ProcessGenuineSubmission(MappingEntity mappingEntity, string form, string sessionGuid, string reference)
        {
            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path);
            _tagParsers.ToList().ForEach(_ => _.Parse(currentPage, mappingEntity.FormAnswers));
            var submitSlug = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

            _logger.LogInformation($"SubmitService:ProcessGenuineSubmission:{sessionGuid} {form} Posting Request");
            HttpResponseMessage response = await _submitProviders.Get(submitSlug.Type).PostAsync(mappingEntity, submitSlug);
            _logger.LogInformation($"SubmitService:ProcessGenuineSubmission:{sessionGuid} {form} Response Received");

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occurred while attempting to call {submitSlug.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");

            if (!mappingEntity.BaseForm.GenerateReferenceNumber && response.Content is not null)
            {
                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                reference = JsonConvert.DeserializeObject<string>(content);
                _pageHelper.SaveCaseReference(sessionGuid, reference);
            }

            return reference;
        }

        public async Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            _logger.LogInformation($"SubmitService.PaymentSubmission:{sessionGuid} Submitting {form}");

            if (_submissionServiceConfiguration.FakeSubmission)
                return ProcessFakeSubmission(mappingEntity, form, sessionGuid, string.Empty);

            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path);

            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

            if (string.IsNullOrEmpty(postUrl.URL))
                throw new ApplicationException($"SubmitService::PaymentSubmission, No submission URL has been provided for FORM: {form}, ENVIRONMENT: {_environment.EnvironmentName}");

            _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(postUrl.AuthToken) ? string.Empty : postUrl.AuthToken);

            _logger.LogInformation($"SubmitService:PaymentSubmission:{sessionGuid} {form} Posting Request");
            var response = await _gateway.PostAsync(postUrl.URL, mappingEntity.Data);
            _logger.LogInformation($"SubmitService:PaymentSubmission:{sessionGuid} {form} Response Received");

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occurred while attempting to call {postUrl.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");

            if (response.Content is not null)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    throw new ApplicationException($"SubmitService::PaymentSubmission, Gateway {postUrl.URL} responded with empty reference");

                var submissionReference = JsonConvert.DeserializeObject<string>(content);

                _pageHelper.SaveCaseReference(sessionGuid, submissionReference);

                _logger.LogInformation($"SubmitService.PaymentSubmission:{sessionGuid} Submitted successfully {form} - {submissionReference}");

                return submissionReference;
            }

            throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occurred when response content from {postUrl} is null, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
        }

        public async Task<string> RedirectSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path);

            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

            if (string.IsNullOrEmpty(postUrl.URL))
                throw new ApplicationException($"SubmitService::RedirectSubmission, No submission URL has been provided for FORM: {form}, ENVIRONMENT: {_environment.EnvironmentName}");

            if (string.IsNullOrEmpty(postUrl.RedirectUrl))
                throw new ApplicationException($"SubmitService::RedirectSubmission, No redirect URL has been provided for FORM: {form}, ENVIRONMENT: {_environment.EnvironmentName}");

            _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(postUrl.AuthToken) ? string.Empty : postUrl.AuthToken);

            string content;

            if (_submissionServiceConfiguration.FakeSubmission)
            {
                content = ProcessFakeSubmission(mappingEntity, form, sessionGuid, string.Empty);
            }
            else
            {
                var response = await _gateway.PostAsync(postUrl.URL, mappingEntity.Data);

                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"SubmitService::RedirectSubmission, An exception has occurred while attempting to call {postUrl.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");

                content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    throw new ApplicationException($"SubmitService::RedirectSubmission, Gateway {postUrl.URL} responded with empty reference");
            }

            mappingEntity.FormAnswers.CaseReference = JsonConvert.DeserializeObject<string>(content);
            return _tagParsers.Aggregate(postUrl.RedirectUrl, (current, tagParser) => tagParser.ParseString(current, mappingEntity.FormAnswers));
        }
    }
}
