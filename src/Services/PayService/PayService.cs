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

namespace form_builder.Services.PayService
{
    public interface IPayService
    {
        Task<string> ProcessPayment(string form, string path, string reference, string sessionGuid);
        Task<string> ProcessPaymentResponse(string form, string responseCode, string reference);

    }

    public class PayService : IPayService
    {
        private readonly IGateway _gateway;
        private readonly ILogger<PayService> _logger;
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly ICache _cache;
        private readonly DistrbutedCacheConfiguration _distrbutedCacheConfiguration;
        private readonly DistrbutedCacheExpirationConfiguration _distrbutedCacheExpirationConfiguration;

        public PayService(IEnumerable<IPaymentProvider> paymentProviders, ILogger<PayService> logger, IGateway gateway, ICache cache, IOptions<DistrbutedCacheExpirationConfiguration> distrbutedCacheExpirationConfiguration, IOptions<DistrbutedCacheConfiguration> distrbutedCacheConfiguration)
        {
            _gateway = gateway;
            _logger = logger;
            _paymentProviders = paymentProviders;
            _cache = cache;
            _distrbutedCacheConfiguration = distrbutedCacheConfiguration.Value;
            _distrbutedCacheExpirationConfiguration = distrbutedCacheExpirationConfiguration.Value;
        }

        public async Task<string> ProcessPayment(string form, string path, string reference, string sessionGuid)
        {
            var paymentInformation = await GetFormPaymentInformation(form);
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            return await paymentProvider.GeneratePaymentUrl(form, path, reference, sessionGuid, paymentInformation);
        }

        public async Task<string> ProcessPaymentResponse(string form, string responseCode, string reference)
        {
            var paymentInformation = await GetFormPaymentInformation(form);
            var paymentProvider = GetFormPaymentProvider(paymentInformation);

            try {
                var result = paymentProvider.VerifyPaymentResponse(responseCode, reference);
                return result;
            } catch(Exception e){
                throw e;
            }
        }

        private async Task<PaymentInformation> GetFormPaymentInformation(string form)
        {
            var paymentInformation = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>("paymentconfiguration", _distrbutedCacheExpirationConfiguration.PaymentConfiguration, _distrbutedCacheConfiguration.UseDistrbutedCache, ESchemaType.PaymentConfiguration);

            var paymentInfo = paymentInformation.Select(x => x)
               .Where(c => c.FormName == form)
               .FirstOrDefault();

            if (paymentInfo == null)
            {
                throw new ApplicationException($"PayService:: No payment information found for {form}");
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
                throw new ApplicationException($"PayService:: No payment provider configure for {paymentInfo.PaymentProvider}");
            }

            return paymentProvider;
        }
    }
}