using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using form_builder.Configuration;
using Microsoft.Extensions.Options;
using form_builder.Providers.PaymentProvider;
using form_builder.Cache;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using Microsoft.AspNetCore.Hosting;
using form_builder.Services.MappingService.Entities;
using Amazon.S3.Model;
using Newtonsoft.Json;

namespace form_builder.Services.PayService
{
    public interface IPayService
    {
        Task<string> ProcessPayment(string form, string path, string reference, string sessionGuid);
        Task<string> ProcessPayment(MappingEntity mappingEntity, string form, string path, string reference, string sessionGuid);
        Task<string> ProcessPaymentResponse(string form, string responseCode, string reference);
        Task<PaymentInformation> GetFormPaymentInformation(string form);
        Task<PaymentInformation> GetFormPaymentInformation(MappingEntity mappingEntity, string form);
    }

    public class PayService : IPayService
    {
        private readonly IGateway _gateway;
        private readonly ILogger<PayService> _logger;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly ICache _cache;
        private readonly DistributedCacheExpirationConfiguration _distrbutedCacheExpirationConfiguration;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMappingService _mappingService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public PayService(IEnumerable<IPaymentProvider> paymentProviders, ILogger<PayService> logger,
            IGateway gateway, ICache cache,
            IOptions<DistributedCacheExpirationConfiguration> distrbutedCacheExpirationConfiguration,
            ISessionHelper sessionHelper, IMappingService mappingService, IHostingEnvironment hostingEnvironment)
        {
            _gateway = gateway;
            _logger = logger;
            _paymentProviders = paymentProviders;
            _cache = cache;
            _distrbutedCacheExpirationConfiguration = distrbutedCacheExpirationConfiguration.Value;
            _sessionHelper = sessionHelper;
            _mappingService = mappingService;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> ProcessPayment(string form, string path, string reference, string sessionGuid)
        {
            var paymentInformation = await GetFormPaymentInformation(form);
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            return await paymentProvider.GeneratePaymentUrl(form, path, reference, sessionGuid, paymentInformation);
        }
        public async Task<string> ProcessPayment(MappingEntity formData, string form, string path, string reference, string sessionGuid)
        {
            var paymentInformation = await GetFormPaymentInformation(formData, form);
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            return await paymentProvider.GeneratePaymentUrl(form, path, reference, sessionGuid, paymentInformation);
        }

        public async Task<string> ProcessPaymentResponse(string form, string responseCode, string reference)
        {
            var paymentInformation = await GetFormPaymentInformation(form);

            var sessionGuid = _sessionHelper.GetSessionGuid();
            var mappingEntity = await _mappingService.Map(sessionGuid, form);
            var currentPage = mappingEntity.BaseForm.GetPage(mappingEntity.FormAnswers.Path);

            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _hostingEnvironment.EnvironmentName.ToS3EnvPrefix());
            var paymentProvider = GetFormPaymentProvider(paymentInformation);
            if (String.IsNullOrWhiteSpace(postUrl.CallbackUrl))
            {
                throw new ArgumentException("PayService::ProcessPaymentResponse, Callback url has not been specified");
            }

            _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
            try
            {
                paymentProvider.VerifyPaymentResponse(responseCode);
                var response = await _gateway.PostAsync(postUrl.CallbackUrl,
                    new {CaseReference = reference, PaymentStatus = EPaymentStatus.Success.ToString()});
                return reference;
            }
            catch (PaymentDeclinedException)
            {
                var response = await _gateway.PostAsync(postUrl.CallbackUrl,
                    new {CaseReference = reference, PaymentStatus = EPaymentStatus.Declined.ToString()});
                throw new PaymentDeclinedException("PayService::ProcessPaymentResponse, PaymentProvider declined payment");
            }
            catch (PaymentFailureException)
            {
                var response = await _gateway.PostAsync(postUrl.CallbackUrl,
                    new {CaseReference = reference, PaymentStatus = EPaymentStatus.Failure.ToString()});
                throw new PaymentFailureException("PayService::ProcessPaymentResponse, PaymentProvider failed payment");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "The payment callback failed");
                throw e;
            }
        }

        public async Task<PaymentInformation> GetFormPaymentInformation(string form)
        {
            var paymentInformation = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>($"paymentconfiguration.{_hostingEnvironment.EnvironmentName}", _distrbutedCacheExpirationConfiguration.PaymentConfiguration, ESchemaType.PaymentConfiguration);

            var paymentInfo = paymentInformation.Select(x => x)
               .Where(c => c.FormName == form)
               .FirstOrDefault();

            if (paymentInfo == null)
            {
                throw new ApplicationException($"PayService:: No payment information found for {form}");
            }

            return paymentInfo;
        }

        public async Task<PaymentInformation> GetFormPaymentInformation(MappingEntity formData, string form)
        {
            var paymentInformation = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>($"paymentconfiguration.{_hostingEnvironment.EnvironmentName}", _distrbutedCacheExpirationConfiguration.PaymentConfiguration, ESchemaType.PaymentConfiguration);

            var paymentInfo = paymentInformation.Select(x => x)
               .Where(c => c.FormName == form)
               .FirstOrDefault();

            if (paymentInfo == null)
            {
                throw new ApplicationException($"PayService:: No payment information found for {form}");
            }

            paymentInfo.Settings.Amount = await CalculateAmountAsync(formData, paymentInfo);

            return paymentInfo;
        }

        private async Task<string> CalculateAmountAsync(MappingEntity formData, PaymentInformation paymentInfo)
        {
            var currentPage = formData.BaseForm.GetPage(formData.FormAnswers.Path);
            var postUrl = currentPage.GetSubmitFormEndpoint(formData.FormAnswers, _hostingEnvironment.EnvironmentName.ToS3EnvPrefix());
            _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
            var response = await _gateway.PostAsync(postUrl.CalculateCostUrl, formData);
            var reference = string.Empty;
            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                reference = JsonConvert.DeserializeObject<string>(content);

            }
            //return reference;

            return reference;
        }

        private IPaymentProvider GetFormPaymentProvider(PaymentInformation paymentInfo)
        {
            var paymentProvider = _paymentProviders.ToList()
                .Where(_ => _.ProviderName == paymentInfo.PaymentProvider)
                .FirstOrDefault();

            if (paymentProvider == null)
            {
                throw new ApplicationException($"PayService:: No payment provider configure for {paymentInfo.PaymentProvider}");
            }

            return paymentProvider;
        }
    }
}