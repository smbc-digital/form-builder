using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.Transforms.PaymentConfiguration;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.PayService
{
    public class PayService : IPayService
    {
        private readonly IGateway _gateway;
        private readonly ILogger<PayService> _logger;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly IPaymentConfigurationTransformDataProvider _paymentConfigProvider;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMappingService _mappingService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPageHelper _pageHelper;

        public PayService(
            IEnumerable<IPaymentProvider> paymentProviders,
            ILogger<PayService> logger,
            IGateway gateway,
            ISessionHelper sessionHelper,
            IMappingService mappingService,
            IWebHostEnvironment hostingEnvironment,
            IPageHelper pageHelper, 
            IPaymentConfigurationTransformDataProvider paymentConfigProvider)
        {
            _gateway = gateway;
            _logger = logger;
            _paymentProviders = paymentProviders;
            _sessionHelper = sessionHelper;
            _mappingService = mappingService;
            _hostingEnvironment = hostingEnvironment;
            _pageHelper = pageHelper;
            _paymentConfigProvider = paymentConfigProvider;
        }

        public async Task<string> ProcessPayment(MappingEntity formData, string form, string path, string reference, string sessionGuid)
        {
            var page = formData.BaseForm.GetPage(_pageHelper, "payment-summary");
            var paymentInformation = await GetFormPaymentInformation(formData, form, page);
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            return await paymentProvider.GeneratePaymentUrl(form, path, reference, sessionGuid, paymentInformation);
        }

        public async Task<string> ProcessPaymentResponse(string form, string responseCode, string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var mappingEntity = await _mappingService.Map(sessionGuid, form);
            if (mappingEntity == null)
                throw new Exception($"PayService:: No mapping entity found for {form}");

            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path);
            var paymentInformation = await GetFormPaymentInformation(mappingEntity, form, currentPage);
            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _hostingEnvironment.EnvironmentName.ToS3EnvPrefix());
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            if (string.IsNullOrWhiteSpace(postUrl.CallbackUrl))
                throw new ArgumentException("PayService::ProcessPaymentResponse, Callback url has not been specified");

            _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
            try
            {
                paymentProvider.VerifyPaymentResponse(responseCode);
                await _gateway.PostAsync(postUrl.CallbackUrl,
                    new { CaseReference = reference, PaymentStatus = EPaymentStatus.Success.ToString() });
                return reference;
            }
            catch (PaymentDeclinedException)
            {
                await _gateway.PostAsync(postUrl.CallbackUrl,
                    new { CaseReference = reference, PaymentStatus = EPaymentStatus.Declined.ToString() });
                throw new PaymentDeclinedException("PayService::ProcessPaymentResponse, PaymentProvider declined payment");
            }
            catch (PaymentFailureException)
            {
                await _gateway.PostAsync(postUrl.CallbackUrl,
                    new { CaseReference = reference, PaymentStatus = EPaymentStatus.Failure.ToString() });
                throw new PaymentFailureException("PayService::ProcessPaymentResponse, PaymentProvider failed payment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The payment callback failed");
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaymentInformation> GetFormPaymentInformation(MappingEntity formData, string form, Page page)
        {
            var paymentConfig = await _paymentConfigProvider.Get<List<PaymentInformation>>();
            var formPaymentConfig = paymentConfig.FirstOrDefault(_ => _.FormName == form);

            if (formPaymentConfig == null)
                throw new Exception($"PayService:: No payment information found for {form}");

            if (string.IsNullOrEmpty(formPaymentConfig.Settings.Amount))
                formPaymentConfig.Settings.Amount = await CalculateAmountAsync(formData, formPaymentConfig);

            return formPaymentConfig;
        }

        private async Task<string> CalculateAmountAsync(MappingEntity formData, PaymentInformation formPaymentConfig)
        {
            try
            {
                var postUrl = formPaymentConfig.Settings.CalculationSlugs.FirstOrDefault(_ =>
                    _.Environment.ToLower().Equals(_hostingEnvironment.EnvironmentName.ToLower()));

                if (postUrl?.URL == null || postUrl.AuthToken == null)
                    throw new Exception($"PayService::CalculateAmountAsync, slug for {_hostingEnvironment.EnvironmentName} not found or incomplete");

                _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
                var response = await _gateway.PostAsync(postUrl.URL, formData.Data);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"PayService::CalculateAmountAsync, Gateway returned unsuccessful status code {response.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

                if (response.Content == null)
                    throw new ApplicationException($"PayService::CalculateAmountAsync, Gateway {postUrl.URL} responded with null content");

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    throw new ApplicationException($"PayService::CalculateAmountAsync, Gateway {postUrl.URL} responded with empty payment amount within content");

                return JsonConvert.DeserializeObject<string>(content);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private IPaymentProvider GetFormPaymentProvider(PaymentInformation paymentInfo)
        {
            var paymentProvider = _paymentProviders.FirstOrDefault(_ => _.ProviderName == paymentInfo.PaymentProvider);

            if (paymentProvider == null)
                throw new Exception($"PayService::GetFormPaymentProvider, No payment provider configured for {paymentInfo.PaymentProvider}");

            return paymentProvider;
        }
    }
}