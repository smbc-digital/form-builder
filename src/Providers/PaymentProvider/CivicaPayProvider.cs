using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.CivicaPay;
using StockportGovUK.NetStandard.Models.Civica.Pay.Request;

namespace form_builder.Providers.PaymentProvider
{
    public class CivicaPayProvider : IPaymentProvider
    {
        public string ProviderName => "CivicaPay";
        private readonly ICivicaPayGateway _civicaPayGateway;
        private readonly CivicaPaymentConfiguration _paymentConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CivicaPayProvider> _logger;

        public CivicaPayProvider(ICivicaPayGateway civicaPayGateway, IOptions<CivicaPaymentConfiguration> paymentConfiguration, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, ILogger<CivicaPayProvider> logger)
        {
            _civicaPayGateway = civicaPayGateway;
            _httpContextAccessor = httpContextAccessor;
            _paymentConfig = paymentConfiguration.Value;
            _environment = environment;
            _logger = logger;
        }
        public async Task<string> GeneratePaymentUrl(string form, string path, string reference, string sessionGuid, PaymentInformation paymentInformation,List<Answers> formAnswers)
        {
            if (string.IsNullOrEmpty(reference))
                throw new PaymentFailureException("CivicaPayProvider::No valid reference");

            AddressDetail address = new();

            if (!string.IsNullOrEmpty(paymentInformation.Settings.AddressReference))
            {
                if (paymentInformation.Settings.AddressReference.Contains(AddressConstants.DESCRIPTION_SUFFIX))
                {
                    address = paymentInformation.Settings.AddressReference.ConvertStringToObject();
                }
                else
                {
                    if (paymentInformation.Settings.AddressReference.Contains("yourAddress-ManualPostcode"))
                    {
                        foreach (var answer in formAnswers)
                        {
                            if (answer.QuestionId.Contains("yourAddress-AddressLine1"))
                                address.Street = answer.Response;

                            if (answer.QuestionId.Contains("yourAddress-AddressTown"))
                                address.Town = answer.Response;

                            if (answer.QuestionId.Contains("yourAddress-ManualPostcode"))
                                address.Postcode = answer.Response;
                        }
                    }
                }
            }
            CreateImmediateBasketRequest basket = new()
            {
                CallingAppIdentifier = "Basket",
                CustomerID = _paymentConfig.CustomerId,
                ApiPassword = _paymentConfig.ApiPassword,
                ReturnURL = $"https://{_httpContextAccessor.HttpContext.Request.Host}{_environment.EnvironmentName.ToReturnUrlPrefix()}/{form}/{path}/payment-response",
                NotifyURL = $"https://{_httpContextAccessor.HttpContext.Request.Host}{_environment.EnvironmentName.ToReturnUrlPrefix()}/{form}/payment-notification",
                CallingAppTranReference = reference, 
                PaymentItems = new List<PaymentItem>
                {
                    new PaymentItem
                    {
                        PaymentDetails = new PaymentDetail
                        {
                            CatalogueID = paymentInformation.Settings.CatalogueId,
                            AccountReference = paymentInformation.Settings.AccountReference,
                            PaymentAmount = paymentInformation.Settings.Amount,
                            Quantity = "1",
                            PaymentNarrative = form,
                            CallingAppTranReference = reference,
                            ServicePayItemDesc = paymentInformation.Settings.Description
                        },
                        AddressDetails = new AddressDetail
                        {
                            HouseNo = address.HouseNo,
                            Street = address.Street,
                            Town = address.Town,
                            Postcode = address.Postcode
                        }
                    }
                }
            };

            _logger.LogError(basket.NotifyURL);

            if (paymentInformation.IsServicePay())
            {
                basket.PaymentItems[0].PaymentDetails.ServicePayReference = paymentInformation.Settings.ServicePayReference;
                basket.PaymentItems[0].PaymentDetails.ServicePayNarrative = paymentInformation.Settings.ServicePayNarrative;
            }

            if (!string.IsNullOrEmpty(paymentInformation.Settings.Name))
                basket.PaymentItems[0].AddressDetails.Name = paymentInformation.Settings.Name;

            if (!string.IsNullOrEmpty(paymentInformation.Settings.Email))
                basket.PaymentItems[0].PaymentDetails.EmailAddress = paymentInformation.Settings.Email;

            var civicaResponse = await _civicaPayGateway.CreateImmediateBasketAsync(basket);

            if (!civicaResponse.IsSuccessStatusCode)
                throw new Exception($"CivicaPayProvider::GeneratePaymentUrl, CivicaPay gateway response with a non ok status code {civicaResponse.StatusCode}, HttpResponse: {JsonConvert.SerializeObject(civicaResponse)}");

            if (civicaResponse.ResponseContent.Success.Equals("false"))
                throw new Exception($"CivicaPayProvider::GeneratePaymentUrl, CivicaPay gateway responded with a non successful response from the provider, { civicaResponse.ResponseContent.ResponseCode }, Summary: { civicaResponse.ResponseContent.ErrorMessage } - { civicaResponse.ResponseContent.ErrorSummary }, HttpResponse: {JsonConvert.SerializeObject(civicaResponse)}");

            return _civicaPayGateway.GetPaymentUrl(civicaResponse.ResponseContent.BasketReference, civicaResponse.ResponseContent.BasketToken, reference);
        }

        public void VerifyPaymentResponse(string responseCode)
        {
            if (responseCode.Equals("00022") || responseCode.Equals("00023") || responseCode.Equals("00001"))
                throw new PaymentDeclinedException($"CivicaPayProvider::Declined payment with response code: {responseCode}");

            if (!responseCode.Equals("00000"))
                throw new PaymentFailureException($"CivicaPayProvider::Payment failed with response code: {responseCode}");
        }
    }
}