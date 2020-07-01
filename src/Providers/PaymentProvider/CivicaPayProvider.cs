using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Exceptions;
using form_builder.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockportGovUK.NetStandard.Gateways.Civica.Pay;
using StockportGovUK.NetStandard.Models.Civica.Pay.Request;

namespace form_builder.Providers.PaymentProvider
{
    public class CivicaPayProvider : IPaymentProvider
    {
        public string ProviderName => "CivicaPay";
        private readonly ICivicaPayGateway _civicaPayGateway;
        private readonly CivicaPaymentConfiguration _paymentConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _environment;
        private readonly ILogger<CivicaPayProvider> _logger;

        public CivicaPayProvider(ICivicaPayGateway civicaPayGateway, IOptions<CivicaPaymentConfiguration> paymentConfiguration, IHttpContextAccessor httpContextAccessor, IHostingEnvironment environment, ILogger<CivicaPayProvider> logger)
        {
            _civicaPayGateway = civicaPayGateway;
            _httpContextAccessor = httpContextAccessor;
            _paymentConfig = paymentConfiguration.Value;
            _environment = environment;
            _logger = logger;
        }
        public async Task<string> GeneratePaymentUrl(string form, string path, string reference, string sessionGuid, PaymentInformation paymentInformation)
        {
            if (String.IsNullOrEmpty(reference))
                throw new PaymentFailureException("CivicaPayProvider::No valid reference");

            var basket = new CreateImmediateBasketRequest
            {
                CallingAppIdentifier = "Basket",
                CustomerID = _paymentConfig.CustomerId,
                ApiPassword = _paymentConfig.ApiPassword,
                ReturnURL = _environment.EnvironmentName.Equals("local") ? $"https://{_httpContextAccessor.HttpContext.Request.Host}{_environment.EnvironmentName.ToReturnUrlPrefix()}/{form}/{path}/payment-response" : $"https://{_httpContextAccessor.HttpContext.Request.Host}{_environment.EnvironmentName.ToReturnUrlPrefix()}/v2/{form}/{path}/payment-response",
                NotifyURL = string.Empty,
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
                        AddressDetails = new AddressDetail()
                    }
                }
            };

            var civicaResponse = await _civicaPayGateway.CreateImmediateBasketAsync(basket);

            if (civicaResponse.StatusCode != HttpStatusCode.OK)
                throw new Exception($"CivicaPayProvider::GeneratePaymentUrl, CivicaPay gateway response with a non ok status code {civicaResponse.StatusCode}, HttpResponse: {civicaResponse}");

            return _civicaPayGateway.GetPaymentUrl(civicaResponse.ResponseContent.BasketReference, civicaResponse.ResponseContent.BasketToken, reference);
        }

        public void VerifyPaymentResponse(string responseCode)
        {
            if (responseCode == "00022" || responseCode == "00023" || responseCode == "00001")
            {
                throw new PaymentDeclinedException($"CivicaPayProvider::Declined payment with response code: {responseCode}");
            }

            if (responseCode != "00000")
            {
                throw new PaymentFailureException($"CivicaPayProvider::Payment failed with response code: {responseCode}");
            }
        }
    }
}