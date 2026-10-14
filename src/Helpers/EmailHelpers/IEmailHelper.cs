namespace form_builder.Helpers.EmailHelpers;

public interface IEmailHelper
{
    Task<EmailConfiguration> GetEmailInformation(string form);
}