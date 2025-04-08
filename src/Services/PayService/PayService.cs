using System.Net;
using form_builder.Configuration;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Helpers.Session;
using form_builder.Providers.PaymentProvider;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.TagParsers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.MailingService;
using StockportGovUK.NetStandard.Gateways.MailingServiceProxy;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;
using StockportGovUK.NetStandard.Gateways.Models.GenericReport;
using StockportGovUK.NetStandard.Gateways.Models.Mail;
using EPaymentStatus = StockportGovUK.NetStandard.Gateways.Models.FormBuilder.EPaymentStatus;

namespace form_builder.Services.PayService
{
    public class PayService : IPayService
    {
        private readonly IGateway _gateway;
        private readonly ILogger<PayService> _logger;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMappingService _mappingService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPageHelper _pageHelper;
        private readonly IPaymentHelper _paymentHelper;
        private readonly PaymentConfiguration _paymentConfiguration;
        private readonly IEnumerable<ITagParser> _tagParsers;
        private readonly IMailingServiceProxyGateway _mailingServiceGateway;
        private readonly ErrorEmailConfiguration _errorEmailConfiguration;

        public PayService(
            IEnumerable<IPaymentProvider> paymentProviders,
            ILogger<PayService> logger,
            IGateway gateway,
            ISessionHelper sessionHelper,
            IMappingService mappingService,
            IWebHostEnvironment hostingEnvironment,
            IPageHelper pageHelper,
            IPaymentHelper paymentHelper,
            IOptions<PaymentConfiguration> paymentConfiguration,
            IEnumerable<ITagParser> tagParsers,
            IMailingServiceProxyGateway mailingServiceGateway,
            IOptions<ErrorEmailConfiguration> errorEmailConfiguration)
        {
            _gateway = gateway;
            _logger = logger;
            _paymentProviders = paymentProviders;
            _sessionHelper = sessionHelper;
            _mappingService = mappingService;
            _hostingEnvironment = hostingEnvironment;
            _pageHelper = pageHelper;
            _paymentHelper = paymentHelper;
            _paymentConfiguration = paymentConfiguration.Value;
            _tagParsers = tagParsers;
            _mailingServiceGateway = mailingServiceGateway;
            _errorEmailConfiguration = errorEmailConfiguration.Value;
        }

        public async Task<string> ProcessPayment(MappingEntity formData, string form, string path, string reference, string cacheKey)
        {
            var formAnswers = _pageHelper.GetSavedAnswers(cacheKey);
            var paymentInformation = JsonConvert.SerializeObject(await _paymentHelper.GetFormPaymentInformation(form, formData.FormAnswers, formData.BaseForm));
            paymentInformation = _tagParsers.Aggregate(paymentInformation, (current, tagParser) => tagParser.ParseString(current, formAnswers));
            var parsedPaymentInformation = JsonConvert.DeserializeObject<PaymentInformation>(paymentInformation);
            var paymentProvider = GetFormPaymentProvider(parsedPaymentInformation);

            _logger.LogInformation($"PayService::ProcessPayment:{cacheKey} {form} - Creating payment request - for {reference}");

            return await paymentProvider.GeneratePaymentUrl(form, path, reference, cacheKey, parsedPaymentInformation);
        }

        public async Task<string> ProcessPaymentResponse(string form, string responseCode, string reference)
        {
            _logger.LogInformation($"PayService::ProcessPaymentResponse: {form} - Payment response received - {responseCode} for {reference}");

            string browserSessionId = _sessionHelper.GetBrowserSessionId();
            string formSessionId = $"{form}::{browserSessionId}";

            if (string.IsNullOrWhiteSpace(formSessionId))
                _logger.LogWarning($"PayService.ProcessPaymentResponse: {form} - Session expired for {reference}");

            var mappingEntity = await _mappingService.Map(formSessionId, form, null, null);
            if (mappingEntity is null)
                throw new Exception($"{nameof(PayService)}::{nameof(ProcessPaymentResponse)} No mapping entity found for {form}");

            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path, form);
            var paymentInformation = await _paymentHelper.GetFormPaymentInformation(form, mappingEntity.FormAnswers, mappingEntity.BaseForm);

            _tagParsers.ToList().ForEach(_ => _.Parse(currentPage, mappingEntity.FormAnswers, mappingEntity.BaseForm));

            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _hostingEnvironment.EnvironmentName.ToS3EnvPrefix());
            
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            if (string.IsNullOrWhiteSpace(postUrl.CallbackUrl))
                throw new ArgumentException($"{nameof(PayService)}::{nameof(ProcessPaymentResponse)}, Callback url has not been specified");

            _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);

            try
            {
                paymentProvider.VerifyPaymentResponse(responseCode);
                HttpResponseMessage callbackResponse = await HandleCallback(EPaymentStatus.Success, reference, postUrl.CallbackUrl);

                if (mappingEntity.BaseForm.ProcessPaymentCallbackResponse && !callbackResponse.IsSuccessStatusCode)
                    throw new PaymentCallbackException(
                        $"{nameof(PayService)}::{nameof(ProcessPaymentResponse)}, " +
                        $"Callback failed for case {reference}: {callbackResponse.ReasonPhrase}");

                _logger.LogInformation($"PayService::ProcessPaymentResponse:{form} - Payment callback handled successfully for {reference}");
            }
            catch (PaymentDeclinedException)
            {
                await HandleCallback(EPaymentStatus.Declined, reference, postUrl.CallbackUrl);
                throw new PaymentDeclinedException(
                    $"{nameof(PayService)}::{nameof(ProcessPaymentResponse)}, " +
                    $"{paymentProvider.ProviderName} {EPaymentStatus.Declined} payment");
            }
            catch (PaymentFailureException)
            {
                await HandleCallback(EPaymentStatus.Failure, reference, postUrl.CallbackUrl);
                throw new PaymentFailureException(
                    $"{nameof(PayService)}::{nameof(ProcessPaymentResponse)}, " +
                    $"{paymentProvider.ProviderName} {EPaymentStatus.Failure} payment");
            }
            catch (PaymentCallbackException ex)
            {
                throw new PaymentCallbackException(ex.Message);
            }

            _pageHelper.SavePaymentAmount(formSessionId, paymentInformation.Settings.Amount, mappingEntity.BaseForm.PaymentAmountMapping);
            return reference;
        }

        private async Task<HttpResponseMessage> HandleCallback(EPaymentStatus paymentStatus, string reference, string callbackUrl)
        {
            try
            {
                var result = await _gateway.PostAsync(callbackUrl, new PostPaymentUpdateRequest { Reference = reference, PaymentStatus = paymentStatus });
                if (!result.IsSuccessStatusCode)
                {
                    string log = $"{nameof(PayService)}::{nameof(HandleCallback)}, " +
                                 $"Payment callback for {paymentStatus} failed with status code: {result.StatusCode}, " +
                                 $"Payment reference {reference}, Response: {JsonConvert.SerializeObject(result)}";

                    _logger.LogError(log);
                    foreach (string recipient in _errorEmailConfiguration.Recipients)
                    {
                        _mailingServiceGateway.Send(new Mail
                        {
                            Payload = JsonConvert.SerializeObject(new GenericReportMailModel
                            {
                                Header = $"Payment callback failure - {_hostingEnvironment.EnvironmentName}",
                                RecipientAddress = recipient,
                                Reference = reference,
                                FormText = new[] { log },
                                Subject = $"Payment callback failure - {_hostingEnvironment.EnvironmentName}"
                            }),
                            Template = EMailTemplate.GenericReport
                        });
                    }
                }

                return result;
            }
            catch (Exception exception)
            {
                string log = $"{nameof(PayService)}::{nameof(HandleCallback)}, " +
                             $"Payment callback to url {callbackUrl} failed. " +
                             $"Payment status was {paymentStatus}, " +
                             $"failed with Exception: {exception.Message}, Payment reference {reference}";

                _logger.LogError(log);
                foreach (string recipient in _errorEmailConfiguration.Recipients)
                {
                    _mailingServiceGateway.Send(new Mail
                    {
                        Payload = JsonConvert.SerializeObject(new GenericReportMailModel
                        {
                            Header = $"Payment callback exception - {_hostingEnvironment.EnvironmentName}",
                            RecipientAddress = recipient,
                            Reference = reference,
                            FormText = new[] { log },
                            Subject = $"Payment callback exception - {_hostingEnvironment.EnvironmentName}"
                        }),
                        Template = EMailTemplate.GenericReport
                    });
                }

                return new HttpResponseMessage(HttpStatusCode.FailedDependency);
            }
        }

        private IPaymentProvider GetFormPaymentProvider(PaymentInformation paymentInfo)
        {
            if (!_paymentProviders.Any())
                throw new Exception(
                    $"{nameof(PayService)}::{nameof(GetFormPaymentProvider)}, " +
                    $"No payment providers are configured");

            if (_paymentConfiguration.FakePayment && _paymentProviders.Any(_ => _.ProviderName.Equals(_paymentConfiguration.FakeProviderName)))
                return _paymentProviders
                            .FirstOrDefault(_ => _.ProviderName.Equals(_paymentConfiguration.FakeProviderName));

            var paymentProvider = _paymentProviders
                                    .FirstOrDefault(_ => _.ProviderName.Equals(paymentInfo.PaymentProvider));

            if (paymentProvider is null)
                throw new Exception(
                    $"{nameof(PayService)}::{nameof(GetFormPaymentProvider)}, " +
                    $"No payment provider configured for {paymentInfo.PaymentProvider}");

            return paymentProvider;
        }
    }
}