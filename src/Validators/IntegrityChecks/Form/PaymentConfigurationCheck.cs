using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Providers.PaymentProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class PaymentConfigurationCheck: IFormSchemaIntegrityCheck
    {
        private IWebHostEnvironment _environment;
        private readonly ICache _cache;
        private IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public PaymentConfigurationCheck(
            IWebHostEnvironment environment,
            ICache cache, IEnumerable<IPaymentProvider> paymentProviders,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration)
        {
            _environment= environment;
            _cache = cache;
            _paymentProviders = paymentProviders;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        }

        public IntegrityCheckResult Validate(Models.FormSchema schema)
        {
            return ValidateAsync(schema).Result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            var containsPayment = schema.Pages.Where(x => x.Behaviours != null)
                .SelectMany(x => x.Behaviours)
                .Any(x => x.BehaviourType == EBehaviourType.SubmitAndPay);

            if (!containsPayment)
                return IntegrityCheckResult.ValidResult;

            List<PaymentInformation> paymentInformation = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>($"paymentconfiguration.{_environment.EnvironmentName}", _distributedCacheExpirationConfiguration.PaymentConfiguration, ESchemaType.PaymentConfiguration);
            PaymentInformation formPaymentInformation = paymentInformation.FirstOrDefault(payment => payment.FormName == schema.BaseURL);

            if (formPaymentInformation == null)
            {
                integrityCheckResult.AddFailureMessage($"No payment information configured.");
            }
            else
            {
                var paymentProvider = _paymentProviders.FirstOrDefault(_ => _.ProviderName == formPaymentInformation.PaymentProvider);

                if (paymentProvider == null)
                {
                    integrityCheckResult.AddFailureMessage($"No payment provider configured for provider '{formPaymentInformation.PaymentProvider}'");
                    return integrityCheckResult;
                }

                if (formPaymentInformation.Settings.ComplexCalculationRequired)
                {
                    var paymentSummaryElement = schema.Pages.SelectMany(_ => _.Elements)
                        .First(_ => _.Type.Equals(EElementType.PaymentSummary));

                    if (!_environment.IsEnvironment("local") && 
                        !paymentSummaryElement.Properties.CalculationSlugs
                            .Where(_ => !_.Environment.Equals("local", StringComparison.OrdinalIgnoreCase))
                            .Any(_ => _.URL.StartsWith("https://")))
                        integrityCheckResult.AddFailureMessage($"PaymentSummary::CalculateCostUrl must start with https");
                }
            }

            return integrityCheckResult;
        }
    }
}
