using EPaymentStatus = StockportGovUK.NetStandard.Gateways.Models.FormBuilder.EPaymentStatus;

namespace form_builder.Services.PayService;

public class PayService(IEnumerable<IPaymentProvider> paymentProviders,
    ILogger<PayService> logger,
    IGateway gateway,
    ISessionHelper sessionHelper,
    IMappingService mappingService,
    IWebHostEnvironment hostingEnvironment,
    IPageHelper pageHelper,
    IPaymentHelper paymentHelper,
    IOptions<PaymentConfiguration> paymentConfiguration,
    IOptions<SubmissionServiceConfiguration> submissionServiceConfiguration,
    IEnumerable<ITagParser> tagParsers,
    IMailingServiceProxyGateway mailingServiceGateway,
    IOptions<ErrorEmailConfiguration> errorEmailConfiguration)
    : IPayService
{
    private readonly PaymentConfiguration _paymentConfiguration = paymentConfiguration.Value;
    private readonly SubmissionServiceConfiguration _submissionServiceConfiguration = submissionServiceConfiguration.Value;
    private readonly ErrorEmailConfiguration _errorEmailConfiguration = errorEmailConfiguration.Value;

    public async Task<string> ProcessPayment(MappingEntity formData, string form, string path, string reference, string cacheKey)
    {
        var formAnswers = pageHelper.GetSavedAnswers(cacheKey);
        var paymentInformation = JsonConvert.SerializeObject(await paymentHelper.GetFormPaymentInformation(form, formData.FormAnswers, formData.BaseForm));
        paymentInformation = tagParsers.Aggregate(paymentInformation, (current, tagParser) => tagParser.ParseString(current, formAnswers));
        var parsedPaymentInformation = JsonConvert.DeserializeObject<PaymentInformation>(paymentInformation);
        var paymentProvider = GetFormPaymentProvider(parsedPaymentInformation);

        logger.LogInformation($"PayService::ProcessPayment:{cacheKey} {form} - Creating payment request - for {reference}");

        return await paymentProvider.GeneratePaymentUrl(form, path, reference, cacheKey, parsedPaymentInformation);
    }

    public async Task<string> ProcessPaymentResponse(string form, string responseCode, string reference)
    {
        logger.LogInformation($"PayService::ProcessPaymentResponse: {form} - Payment response received - {responseCode} for {reference}");

        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string formSessionId = $"{form}::{browserSessionId}";

        if (string.IsNullOrWhiteSpace(formSessionId))
            logger.LogWarning($"PayService.ProcessPaymentResponse: {form} - Session expired for {reference}");

        var mappingEntity = await mappingService.Map(formSessionId, form, null, null);
        if (mappingEntity is null)
            throw new Exception($"{nameof(PayService)}::{nameof(ProcessPaymentResponse)} No mapping entity found for {form}");

        var currentPage = mappingEntity.BaseForm.GetPage(pageHelper, mappingEntity.FormAnswers.Path, form);
        var paymentInformation = await paymentHelper.GetFormPaymentInformation(form, mappingEntity.FormAnswers, mappingEntity.BaseForm);

        tagParsers.ToList().ForEach(_ => _.Parse(currentPage, mappingEntity.FormAnswers, mappingEntity.BaseForm));

        var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, hostingEnvironment.EnvironmentName.ToS3EnvPrefix());
            
        var paymentProvider = GetFormPaymentProvider(paymentInformation);

        if (string.IsNullOrWhiteSpace(postUrl.CallbackUrl))
            throw new ArgumentException($"{nameof(PayService)}::{nameof(ProcessPaymentResponse)}, Callback url has not been specified");

        gateway.ChangeAuthenticationHeader(postUrl.AuthToken);

        try
        {
            paymentProvider.VerifyPaymentResponse(responseCode);
            HttpResponseMessage callbackResponse = await HandleCallback(EPaymentStatus.Success, reference, postUrl.CallbackUrl);

            if (mappingEntity.BaseForm.ProcessPaymentCallbackResponse && !callbackResponse.IsSuccessStatusCode)
                throw new PaymentCallbackException(
                    $"{nameof(PayService)}::{nameof(ProcessPaymentResponse)}, " +
                    $"Callback failed for case {reference}: {callbackResponse.ReasonPhrase}");

            logger.LogInformation($"PayService::ProcessPaymentResponse:{form} - Payment callback handled successfully for {reference}");
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

        pageHelper.SavePaymentAmount(formSessionId, paymentInformation.Settings.Amount, mappingEntity.BaseForm.PaymentAmountMapping);
        return reference;
    }

    private async Task<HttpResponseMessage> HandleCallback(EPaymentStatus paymentStatus, string reference, string callbackUrl)
    {
        if (_submissionServiceConfiguration.FakeSubmission)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

        try
        {
            var result = await gateway.PostAsync(callbackUrl, new PostPaymentUpdateRequest { Reference = reference, PaymentStatus = paymentStatus });
            if (!result.IsSuccessStatusCode)
            {
                string log = $"{nameof(PayService)}::{nameof(HandleCallback)}, " +
                             $"Payment callback for {paymentStatus} failed with status code: {result.StatusCode}, " +
                             $"Payment reference {reference}, Response: {JsonConvert.SerializeObject(result)}";

                logger.LogError(log);
                foreach (string recipient in _errorEmailConfiguration.Recipients)
                {
                    mailingServiceGateway.Send(new Mail
                    {
                        Payload = JsonConvert.SerializeObject(new GenericReportMailModel
                        {
                            Header = $"Payment callback failure - {hostingEnvironment.EnvironmentName}",
                            RecipientAddress = recipient,
                            Reference = reference,
                            FormText = new[] { log },
                            Subject = $"Payment callback failure - {hostingEnvironment.EnvironmentName}"
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

            logger.LogError(log);
            foreach (string recipient in _errorEmailConfiguration.Recipients)
            {
                mailingServiceGateway.Send(new Mail
                {
                    Payload = JsonConvert.SerializeObject(new GenericReportMailModel
                    {
                        Header = $"Payment callback exception - {hostingEnvironment.EnvironmentName}",
                        RecipientAddress = recipient,
                        Reference = reference,
                        FormText = new[] { log },
                        Subject = $"Payment callback exception - {hostingEnvironment.EnvironmentName}"
                    }),
                    Template = EMailTemplate.GenericReport
                });
            }

            return new HttpResponseMessage(HttpStatusCode.FailedDependency);
        }
    }

    private IPaymentProvider GetFormPaymentProvider(PaymentInformation paymentInfo)
    {
        if (!paymentProviders.Any())
            throw new Exception(
                $"{nameof(PayService)}::{nameof(GetFormPaymentProvider)}, " +
                $"No payment providers are configured");

        if (_paymentConfiguration.FakePayment && paymentProviders.Any(_ => _.ProviderName.Equals(_paymentConfiguration.FakeProviderName)))
            return paymentProviders
                .FirstOrDefault(_ => _.ProviderName.Equals(_paymentConfiguration.FakeProviderName));

        var paymentProvider = paymentProviders
            .FirstOrDefault(_ => _.ProviderName.Equals(paymentInfo.PaymentProvider));

        if (paymentProvider is null)
            throw new Exception(
                $"{nameof(PayService)}::{nameof(GetFormPaymentProvider)}, " +
                $"No payment provider configured for {paymentInfo.PaymentProvider}");

        return paymentProvider;
    }
}