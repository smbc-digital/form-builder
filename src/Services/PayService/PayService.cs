﻿using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json;
using form_builder.Models;

namespace form_builder.Services.PayService
{
    public interface IPayService
    {
        Task<string> ProcessPayment(MappingEntity mappingEntity, string form, string path, string reference, string sessionGuid);
        Task<string> ProcessPaymentResponse(string form, string responseCode, string reference);
        Task<PaymentInformation> GetFormPaymentInformation(MappingEntity mappingEntity, string form, Page page);
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

        public async Task<string> ProcessPayment(MappingEntity formData, string form, string path, string reference, string sessionGuid)
        {
            var page = formData.BaseForm.GetPage("payment-summary");
            var paymentInformation = await GetFormPaymentInformation(formData, form, page);
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            return await paymentProvider.GeneratePaymentUrl(form, path, reference, sessionGuid, paymentInformation);
        }

        public async Task<string> ProcessPaymentResponse(string form, string responseCode, string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var mappingEntity = await _mappingService.Map(sessionGuid, form);
            if (mappingEntity == null)
            {
                throw new Exception($"PayService:: No mapping entity found for {form}");
            }

            var currentPage = mappingEntity.BaseForm.GetPage(mappingEntity.FormAnswers.Path);
            var paymentInformation = await GetFormPaymentInformation(mappingEntity, form, currentPage);
            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _hostingEnvironment.EnvironmentName.ToS3EnvPrefix());
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            if (string.IsNullOrWhiteSpace(postUrl.CallbackUrl))
            {
                throw new ArgumentException("PayService::ProcessPaymentResponse, Callback url has not been specified");
            }

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
                var response = await _gateway.PostAsync(postUrl.CallbackUrl,
                    new { CaseReference = reference, PaymentStatus = EPaymentStatus.Declined.ToString() });
                throw new PaymentDeclinedException("PayService::ProcessPaymentResponse, PaymentProvider declined payment");
            }
            catch (PaymentFailureException)
            {
                var response = await _gateway.PostAsync(postUrl.CallbackUrl,
                    new { CaseReference = reference, PaymentStatus = EPaymentStatus.Failure.ToString() });
                throw new PaymentFailureException("PayService::ProcessPaymentResponse, PaymentProvider failed payment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The payment callback failed");
                throw ex;
            }
        }

        public async Task<PaymentInformation> GetFormPaymentInformation(MappingEntity formData, string form, Page page)
        {
            var paymentConfig = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>($"paymentconfiguration.{_hostingEnvironment.EnvironmentName}", _distrbutedCacheExpirationConfiguration.PaymentConfiguration, ESchemaType.PaymentConfiguration);
            var formPaymentConfig = paymentConfig.FirstOrDefault(_ => _.FormName == form);

            if (formPaymentConfig == null)
            {
                throw new Exception($"PayService:: No payment information found for {form}");
            }

            if (formPaymentConfig.Settings.ComplexCalculationRequired)
            {
                formPaymentConfig.Settings.Amount = await CalculateAmountAsync(formData, page);
            }

            return formPaymentConfig;
        }

        private async Task<string> CalculateAmountAsync(MappingEntity formData, Page page)
        {
            try
            {
                var paymentSummary = page.Elements.FirstOrDefault(_ => _.Type == EElementType.PaymentSummary);
                var postUrl = paymentSummary.Properties.CalculationSlugs.FirstOrDefault(_ =>
                    _.Environment == _hostingEnvironment.EnvironmentName);

                _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
                var response = await _gateway.PostAsync(postUrl.URL, formData.Data);
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new Exception("PayService:: Gateway returned InternalServerError");
                }

                var amount = string.Empty;
                if (response.Content == null) return amount;

                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                amount = JsonConvert.DeserializeObject<string>(content);

                return amount;
            }
            catch (Exception ex)
            {
                throw new Exception($"PayService:: Payment information could not be calculated: {ex}");
            }
        }

        private IPaymentProvider GetFormPaymentProvider(PaymentInformation paymentInfo)
        {
            var paymentProvider = _paymentProviders.FirstOrDefault(_ => _.ProviderName == paymentInfo.PaymentProvider);

            if (paymentProvider == null)
            {
                throw new Exception($"PayService:: No payment provider configured for {paymentInfo.PaymentProvider}");
            }

            return paymentProvider;
        }
    }
}