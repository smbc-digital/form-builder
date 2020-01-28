using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using form_builder.Configuration;
using Microsoft.Extensions.Options;
using form_builder.Providers.PaymentProvider;

namespace form_builder.Services.PayService
{
    public interface IPayService
    {
        Task<string> ProcessPayment(string form, string path, string reference, string sessionGuid);
        string ProcessPaymentResponse(string form, string responseCode);
    }

    public class PayService : IPayService
    {
        private readonly IGateway _gateway;
        private readonly ILogger<PayService> _logger;
        private readonly PaymentInformationConfiguration _paymentInformationConfig;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;

        public PayService(IEnumerable<IPaymentProvider> paymentProviders, ILogger<PayService> logger, IGateway gateway, IOptions<PaymentInformationConfiguration> paymentInformationConfiguration)
        {
            _gateway = gateway;
            _logger = logger;
            _paymentInformationConfig = paymentInformationConfiguration.Value;
            _paymentProviders = paymentProviders;
        }

        public async Task<string> ProcessPayment(string form, string path, string reference, string sessionGuid)
        {
            var paymentInformation = GetFormPaymentInformation(form);
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            return await paymentProvider.GeneratePaymentUrl(form, path, reference, sessionGuid, paymentInformation);
        }

        public string ProcessPaymentResponse(string form, string responseCode)
        {
            var paymentInformation = GetFormPaymentInformation(form);
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            try {
                return paymentProvider.VerifyPaymentResponse(responseCode);
            } catch(Exception e){
                throw e;
            }
        }

        private PaymentInformation GetFormPaymentInformation(string form)
        {
            var paymentInfo = _paymentInformationConfig.PaymentConfigs.Select(x => x)
               .Where(c => c.FormName == form)
               .FirstOrDefault();

            if (paymentInfo == null)
            {
                throw new ApplicationException($"PayService::ProcessPaymentResponse: No payment information found for {form}");
            }

            return paymentInfo;
        }

        private IPaymentProvider GetFormPaymentProvider(PaymentInformation paymentInfo)
        {
            var paymentProvider = _paymentProviders.ToList()
                .Where(_ => _.ProviderName == paymentInfo.PaymentProvider)
                .FirstOrDefault();

            if (paymentProvider == null)
            {
                throw new ApplicationException($"PayService::ProcessPaymentResponse: No payment provider configure for {paymentInfo.PaymentProvider}");
            }

            return paymentProvider;
        }
    }
}