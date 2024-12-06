using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Helpers.Submit;
using form_builder.Models;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Submit;
using form_builder.Services.MappingService.Entities;
using form_builder.TagParsers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.SubmitService;

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
    private readonly ISubmitHelper _submitHelper;
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
        ISubmitHelper submitHelper,
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
        _submitHelper = submitHelper;
        _tagParsers = tagParsers;
        _logger = logger;
    }

    public async Task PreProcessSubmission(string form, string cacheKey)
    {
        var baseForm = await _schemaFactory.Build(form);
        if (baseForm.GenerateReferenceNumber)
            _pageHelper.SaveCaseReference(cacheKey, _referenceNumberProvider.GetReference(baseForm.ReferencePrefix), true, baseForm.GeneratedReferenceNumberMapping);

        if (baseForm.SavePaymentAmount)
            _pageHelper.SavePaymentAmount(cacheKey, _paymentHelper.GetFormPaymentInformation(baseForm.BaseURL).Result.Settings.Amount, baseForm.PaymentAmountMapping);
    }

    public async Task<string> ProcessSubmission(MappingEntity mappingEntity, string form, string cacheKey)
    {
        var reference = string.Empty;

        if (mappingEntity.BaseForm.GenerateReferenceNumber)
        {
            var answers = JsonConvert.DeserializeObject<FormAnswers>(_distributedCache.GetString(cacheKey));
            reference = answers.CaseReference;
        }

        if (mappingEntity.BaseForm.Pages.Any(page => page.Elements.Any(element => element.Type.Equals(EElementType.Booking) && element.Properties.AutoConfirm)))
            await _submitHelper.ConfirmBookings(mappingEntity, _environment.EnvironmentName, reference);

        _logger.LogInformation($"SubmitService.ProcessSubmission:{cacheKey} Submitting {form}");
        var submissionReference = _submissionServiceConfiguration.FakeSubmission
            ? ProcessFakeSubmission(mappingEntity, form, cacheKey, reference)
            : await ProcessGenuineSubmission(mappingEntity, form, cacheKey, reference);
            
        _logger.LogInformation($"SubmitService.ProcessSubmission:{cacheKey} Submitted successfully {form} - {submissionReference}");
        return submissionReference;
    }

    private string ProcessFakeSubmission(MappingEntity mappingEntity, string form, string cacheKey, string reference)
    {
        if (!string.IsNullOrEmpty(reference))
            return reference;

        _pageHelper.SaveCaseReference(cacheKey, "123456");
        return "123456";
    }

    private async Task<string> ProcessGenuineSubmission(MappingEntity mappingEntity, string form, string cacheKey, string reference)
    {
        var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path, form);
        _tagParsers.ToList().ForEach(_ => _.Parse(currentPage, mappingEntity.FormAnswers));
        var submitSlug = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

        _logger.LogInformation($"SubmitService:ProcessGenuineSubmission:{cacheKey} {form} Posting Request");

        HttpResponseMessage response = await _submitProviders.Get(submitSlug.Type).PostAsync(mappingEntity, submitSlug);
        _logger.LogInformation($"SubmitService:ProcessGenuineSubmission:{cacheKey} {form} Response Received");

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occurred while attempting to call {submitSlug.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");

        if (!mappingEntity.BaseForm.GenerateReferenceNumber && response.Content is not null)
        {
            var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
            reference = JsonConvert.DeserializeObject<string>(content);
            _pageHelper.SaveCaseReference(cacheKey, reference);
        }

        return reference;
    }

    public async Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string cacheKey)
    {
        _logger.LogInformation($"SubmitService.PaymentSubmission:{cacheKey} Submitting {form}");

        if (_submissionServiceConfiguration.FakeSubmission)
            return ProcessFakeSubmission(mappingEntity, form, cacheKey, string.Empty);

        var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path, form);

        var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

        if (string.IsNullOrEmpty(postUrl.URL))
            throw new ApplicationException($"SubmitService::PaymentSubmission, No submission URL has been provided for FORM: {form}, ENVIRONMENT: {_environment.EnvironmentName}");

        _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(postUrl.AuthToken) ? string.Empty : postUrl.AuthToken);

        _logger.LogInformation($"SubmitService:PaymentSubmission:{cacheKey} {form} Posting Request");
        var response = await _gateway.PostAsync(postUrl.URL, mappingEntity.Data);
        _logger.LogInformation($"SubmitService:PaymentSubmission:{cacheKey} {form} Response Received");

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occurred while attempting to call {postUrl.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");

        if (response.Content is not null)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                throw new ApplicationException($"SubmitService::PaymentSubmission, Gateway {postUrl.URL} responded with empty reference");

            var submissionReference = JsonConvert.DeserializeObject<string>(content);

            _pageHelper.SaveCaseReference(cacheKey, submissionReference);

            _logger.LogInformation($"SubmitService.PaymentSubmission:{cacheKey} Submitted successfully {form} - {submissionReference}");

            return submissionReference;
        }

        throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occurred when response content from {postUrl} is null, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
    }

    public async Task<string> RedirectSubmission(MappingEntity mappingEntity, string form, string cacheKey)
    {
        var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path, form);

        var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

        if (string.IsNullOrEmpty(postUrl.URL))
            throw new ApplicationException($"SubmitService::RedirectSubmission, No submission URL has been provided for FORM: {form}, ENVIRONMENT: {_environment.EnvironmentName}");

        if (string.IsNullOrEmpty(postUrl.RedirectUrl))
            throw new ApplicationException($"SubmitService::RedirectSubmission, No redirect URL has been provided for FORM: {form}, ENVIRONMENT: {_environment.EnvironmentName}");

        _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(postUrl.AuthToken) ? string.Empty : postUrl.AuthToken);

        string content;

        if (_submissionServiceConfiguration.FakeSubmission)
        {
            content = ProcessFakeSubmission(mappingEntity, form, cacheKey, string.Empty);
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

    public async Task<string> ProcessWithoutSubmission(MappingEntity mappingEntity, string form, string cacheKey)
    {
        var answers = JsonConvert.DeserializeObject<FormAnswers>(_distributedCache.GetString(cacheKey));
        var reference = answers.CaseReference;

        if (mappingEntity.BaseForm.Pages.Any(page => page.Elements.Any(element => element.Type.Equals(EElementType.Booking) && element.Properties.AutoConfirm)))
            await _submitHelper.ConfirmBookings(mappingEntity, _environment.EnvironmentName, reference);

        return reference;
    }
}