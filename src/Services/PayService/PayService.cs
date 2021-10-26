﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Helpers.Session;
using form_builder.Providers.PaymentProvider;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.TagParsers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Models.FormBuilder;

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
        private readonly IEnumerable<ITagParser> _tagParsers;

        public PayService(
            IEnumerable<IPaymentProvider> paymentProviders,
            ILogger<PayService> logger,
            IGateway gateway,
            ISessionHelper sessionHelper,
            IMappingService mappingService,
            IWebHostEnvironment hostingEnvironment,
            IPageHelper pageHelper,
            IPaymentHelper paymentHelper, 
            IEnumerable<ITagParser> tagParsers)
        {
            _gateway = gateway;
            _logger = logger;
            _paymentProviders = paymentProviders;
            _sessionHelper = sessionHelper;
            _mappingService = mappingService;
            _hostingEnvironment = hostingEnvironment;
            _pageHelper = pageHelper;
            _paymentHelper = paymentHelper;
            _tagParsers = tagParsers;
        }

        public async Task<string> ProcessPayment(MappingEntity formData, string form, string path, string reference, string sessionGuid)
        {
            var formAnswers = _pageHelper.GetSavedAnswers(sessionGuid);
            var paymentInformation = JsonConvert.SerializeObject(await _paymentHelper.GetFormPaymentInformation(form));
            paymentInformation = _tagParsers.Aggregate(paymentInformation, (current, tagParser) => tagParser.ParseString(current, formAnswers));
            var parsedPaymentInformation = JsonConvert.DeserializeObject<PaymentInformation>(paymentInformation);
            var paymentProvider = GetFormPaymentProvider(parsedPaymentInformation);

            return await paymentProvider.GeneratePaymentUrl(form, path, reference, sessionGuid, parsedPaymentInformation);
        }

        public async Task<string> ProcessPaymentResponse(string form, string responseCode, string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var mappingEntity = await _mappingService.Map(sessionGuid, form);
            if (mappingEntity is null)
                throw new Exception($"PayService:: No mapping entity found for {form}");

            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path);
            var paymentInformation = await _paymentHelper.GetFormPaymentInformation(form);
            _tagParsers.ToList().ForEach(_ => _.Parse(currentPage, mappingEntity.FormAnswers));
            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _hostingEnvironment.EnvironmentName.ToS3EnvPrefix());
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            if (string.IsNullOrWhiteSpace(postUrl.CallbackUrl))
                throw new ArgumentException("PayService::ProcessPaymentResponse, Callback url has not been specified");

            _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
            try
            {
                paymentProvider.VerifyPaymentResponse(responseCode);
                await HandleCallback(EPaymentStatus.Success, reference, postUrl.CallbackUrl);
            }
            catch (PaymentDeclinedException)
            {
                await HandleCallback(EPaymentStatus.Declined, reference, postUrl.CallbackUrl);
                throw new PaymentDeclinedException("PayService::ProcessPaymentResponse, PaymentProvider declined payment");
            }
            catch (PaymentFailureException)
            {
                await HandleCallback(EPaymentStatus.Failure, reference, postUrl.CallbackUrl);
                throw new PaymentFailureException("PayService::ProcessPaymentResponse, PaymentProvider failed payment");
            }

             _pageHelper.SavePaymentAmount(sessionGuid, paymentInformation.Settings.Amount, mappingEntity.BaseForm.PaymentAmountMapping);
            return reference;
        }

        private async Task HandleCallback(EPaymentStatus paymentStatus, string reference, string callbackUrl)
        {
            try {
                var result = await _gateway.PostAsync(callbackUrl, new PostPaymentUpdateRequest{ Reference = reference, PaymentStatus = paymentStatus });

                LogErrorIfCallbackFails(result, reference, EPaymentCallbackStatus.Failure);
            } catch(Exception e){
                _logger.LogError($"PayService::HandleCallback, Payment callback to url {callbackUrl} failed. Payment status was {paymentStatus}, failed with Exception: {e.Message}, Payment reference {reference}");
            }
        }

        private IPaymentProvider GetFormPaymentProvider(PaymentInformation paymentInfo)
        {
            var paymentProvider = _paymentProviders.FirstOrDefault(_ => _.ProviderName.Equals(paymentInfo.PaymentProvider));

            if (paymentProvider is null)
                throw new Exception($"PayService::GetFormPaymentProvider, No payment provider configured for {paymentInfo.PaymentProvider}");

            return paymentProvider;
        }

        private void LogErrorIfCallbackFails(HttpResponseMessage callbackResponse, string reference, EPaymentCallbackStatus callbackType) 
        {
            if(!callbackResponse.IsSuccessStatusCode)
                _logger.LogError($"PayService::ProcessPaymentResponse, Payment callback for {callbackType} failed with statuscode: {callbackResponse.StatusCode}, Payment reference {reference}, Response: {JsonConvert.SerializeObject(callbackResponse)}");
        }
    }
}