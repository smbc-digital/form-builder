using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.Transforms.PaymentConfiguration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class PaymentConfigurationCheck : IFormSchemaIntegrityCheck
    {
        private IWebHostEnvironment _environment;
        private readonly IPaymentConfigurationTransformDataProvider _paymentConfigProvider;
        private IEnumerable<IPaymentProvider> _paymentProviders;

        public PaymentConfigurationCheck(
            IWebHostEnvironment environment, 
            IEnumerable<IPaymentProvider> paymentProviders,
            IPaymentConfigurationTransformDataProvider paymentConfigProvider)
        {
            _environment = environment;
            _paymentProviders = paymentProviders;
            _paymentConfigProvider = paymentConfigProvider;
        }

        public IntegrityCheckResult Validate(FormSchema schema) => ValidateAsync(schema).Result;

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            bool containsPayment = schema.Pages
                .Where(page => page.Behaviours is not null)
                .SelectMany(page => page.Behaviours)
                .Any(page => page.BehaviourType.Equals(EBehaviourType.SubmitAndPay));

            if (!containsPayment)
                return result;

            List<PaymentInformation> paymentInformation = await _paymentConfigProvider.Get<List<PaymentInformation>>();
            PaymentInformation formPaymentInformation = paymentInformation.FirstOrDefault(payment => payment.FormName.Equals(schema.BaseURL));

            if (formPaymentInformation is null)
            {
                result.AddFailureMessage($"No payment information configured.");
                return result;
            }

            IPaymentProvider paymentProvider = _paymentProviders
                .FirstOrDefault(provider => provider.ProviderName
                .Equals(formPaymentInformation.PaymentProvider));

            if (paymentProvider is null)
            {
                result.AddFailureMessage($"No payment provider configured for provider '{formPaymentInformation.PaymentProvider}'");
                return result;
            }

            if (formPaymentInformation.Settings.ComplexCalculationRequired)
            {
                var paymentSummaryElement = schema.Pages
                    .SelectMany(page => page.Elements)
                    .First(element => element.Type.Equals(EElementType.PaymentSummary));

                if (!_environment.IsEnvironment("local") &&
                    !paymentSummaryElement.Properties.CalculationSlugs
                        .Where(submitSlug => !submitSlug.Environment.Equals("local", StringComparison.OrdinalIgnoreCase))
                        .Any(submitSlug => submitSlug.URL.StartsWith("https://")))
                    result.AddFailureMessage($"PaymentSummary::CalculateCostUrl must start with https");
            }

            return result;
        }
    }
}
