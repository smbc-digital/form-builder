using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.Transforms.PaymentConfiguration;

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
                .Any(page => page.BehaviourType.Equals(EBehaviourType.SubmitAndPay)) || schema.SavePaymentAmount;

            if (!containsPayment)
                return result;

            List<PaymentInformation> paymentInformation = await _paymentConfigProvider.Get<List<PaymentInformation>>();
            PaymentInformation formPaymentInformation = paymentInformation
                .FirstOrDefault(payment => payment.FormName
                    .Any(_ => _.Equals(schema.BaseURL)));

            if (formPaymentInformation is null)
            {
                result.AddFailureMessage($"PaymentConfiguration::No payment information configured.");
                return result;
            }

            IPaymentProvider paymentProvider = _paymentProviders
                .FirstOrDefault(provider => provider.ProviderName
                    .Equals(formPaymentInformation.PaymentProvider));

            if (paymentProvider is null)
            {
                result.AddFailureMessage(
                    "PaymentConfiguration::" +
                    $"No payment provider configured for provider '{formPaymentInformation.PaymentProvider}'");

                return result;
            }

            var paymentSetting = formPaymentInformation.Settings;
            if (paymentSetting.CalculationSlug is not null &&
                !string.IsNullOrEmpty(paymentSetting.Amount))
            {
                result.AddFailureMessage("PaymentConfiguration::Only amount or calculationSlug can be provided");
                return result;
            }

            if (paymentSetting.CalculationSlug is null &&
                string.IsNullOrEmpty(paymentSetting.Amount))
            {
                result.AddFailureMessage("PaymentConfiguration::Either amount or calculationSlugs must be provided");
                return result;
            }

            if (paymentSetting.CalculationSlug is not null)
            {
                if (!_environment.IsEnvironment("local") && !paymentSetting.CalculationSlug.URL.StartsWith("https://"))
                    result.AddFailureMessage("PaymentConfiguration::CalculateCostUrl must start with https");
            }

            if (!string.IsNullOrEmpty(paymentSetting.ServicePayReference) &&
                string.IsNullOrEmpty(paymentSetting.ServicePayNarrative))
            {
                result.AddFailureMessage("PaymentConfiguration::If ServicePayReference is used, ServicePayNarrative cannot be empty");
                return result;
            }

            if (!string.IsNullOrEmpty(paymentSetting.ServicePayNarrative) &&
                string.IsNullOrEmpty(paymentSetting.ServicePayReference))
            {
                result.AddFailureMessage("PaymentConfiguration::If ServicePayNarrative is used, ServicePayReference cannot be empty");
                return result;
            }

            var questionIdExistInPaymentConfig = !schema.Pages.Any(page => page.Elements.Any(element => element.Type.Equals(EElementType.Address) &&
                element.Properties.QuestionId.Equals(paymentSetting.AddressReference.RemoveTagParsingFromQuestionId())));

            if (!string.IsNullOrEmpty(paymentSetting.AddressReference) && questionIdExistInPaymentConfig)
            {
                result.AddFailureMessage("PaymentConfiguration::AddressReference QuestionId on Address element must match with AddressReference in payment-config");
                return result;
            }

            return result;
        }
    }
}
