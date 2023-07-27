using form_builder.Configuration;
using form_builder.Providers.Transforms.EmailConfiguration;

namespace form_builder.Helpers.EmailHelpers
{
    public class EmailHelper : IEmailHelper
    {

        private readonly IEmailConfigurationTransformDataProvider _emailConfigProvider;

        public EmailHelper(IEmailConfigurationTransformDataProvider emailConfigProvider) => _emailConfigProvider = emailConfigProvider;

        public async Task<EmailConfiguration> GetEmailInformation(string form)
        {
            var emailConfigList = await _emailConfigProvider.Get<List<EmailConfiguration>>();
            var formEmailConfig = emailConfigList.FirstOrDefault(_ => _.FormName.Any(_ => _.Equals(form)));

            if (formEmailConfig is null)
                throw new Exception($"{nameof(EmailHelper)}::{nameof(GetEmailInformation)}: No email information found for {form}");

            return formEmailConfig;
        }
    }
}
