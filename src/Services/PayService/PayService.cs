using form_builder.Helpers.PageHelpers;
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
    }

    public class PayService : IPayService
    {
        private readonly IGateway _gateway;
        private readonly IPageHelper _pageHelper;
        private readonly ILogger<PayService> _logger;
        private readonly PaymentInformationConfiguration _paymentInformationConfig;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;

        public PayService(IEnumerable<IPaymentProvider> paymentProviders, ILogger<PayService> logger, IGateway gateway, IPageHelper pageHelper, IOptions<PaymentInformationConfiguration> paymentInformationConfiguration)
        {
            _gateway = gateway;
            _pageHelper = pageHelper;
            _logger = logger;
            _paymentInformationConfig = paymentInformationConfiguration.Value;
            _paymentProviders = paymentProviders;
        }

        public async Task<string> ProcessPayment(string form, string path, string reference, string sessionGuid)
        {
            var paymentInfo = _paymentInformationConfig.PaymentConfigs.Select(x => x)
                .Where(c => c.FormName == form)
                .FirstOrDefault();

            var paymentProvider = _paymentProviders.ToList()
                .Where(_ => _.ProviderName == paymentInfo.PaymentProvider)
                .FirstOrDefault();

            if (paymentProvider == null)
            {
                throw new ApplicationException($"No payment provider configure for {paymentInfo.PaymentProvider}");
            }

           return await paymentProvider.GeneratePaymentUrl(form, path, reference, sessionGuid, paymentInfo);
        }
    }
}