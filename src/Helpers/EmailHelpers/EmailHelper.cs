namespace form_builder.Helpers.EmailHelpers;

public class EmailHelper(IEmailConfigurationTransformDataProvider emailConfigProvider)
    : IEmailHelper
{
    public async Task<EmailConfiguration> GetEmailInformation(string form)
    {
        var emailConfigList = await emailConfigProvider.Get<List<EmailConfiguration>>();
        var formEmailConfig = emailConfigList.FirstOrDefault(_ => _.FormName.Any(_ => _.Equals(form)));

        if (formEmailConfig is null)
            throw new Exception($"{nameof(EmailHelper)}::{nameof(GetEmailInformation)}: No email information found for {form}");

        return formEmailConfig;
    }
}