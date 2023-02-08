using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Providers.Transforms.EmailConfiguration;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class EmailConfigurationCheck : IFormSchemaIntegrityCheck
    {
        private readonly IEmailConfigurationTransformDataProvider _emailConfigProvider;

        public EmailConfigurationCheck(IEmailConfigurationTransformDataProvider emailConfigProvider) =>
            _emailConfigProvider = emailConfigProvider;

        public IntegrityCheckResult Validate(FormSchema schema) => ValidateAsync(schema).Result;

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            bool containsEmail = schema.Pages
                .Where(page => page.Behaviours is not null)
                .SelectMany(page => page.Behaviours)
                .Any(page => page.BehaviourType.Equals(EBehaviourType.SubmitEmail));

            if (!containsEmail)
                return result;

            List<EmailConfiguration> emailConfig = await _emailConfigProvider.Get<List<EmailConfiguration>>();
            EmailConfiguration formEmailConfig = emailConfig
                .FirstOrDefault(email => email.FormName
                    .Any(_ => _.Equals(schema.BaseURL.ToLower())));

            if (formEmailConfig is null)
            {
                result.AddFailureMessage($"EmailConfiguration::No email configuration found for {schema.BaseURL}");
                return result;
            }

            if (string.IsNullOrEmpty(formEmailConfig.Recipient.First()))
            {
                result.AddFailureMessage($"EmailConfiguration::No recipient provided for {schema.BaseURL}");
                return result;
            }

            if (string.IsNullOrEmpty(formEmailConfig.Subject))
            {
                result.AddFailureMessage($"EmailConfiguration::No subject provided for {schema.BaseURL}");
                return result;
            }

            return result;
        }
    }
}
