namespace form_builder.Providers.EmailProvider;

public class AwsSesProvider(IAmazonSimpleEmailService emailService) : IEmailProvider
{
    public async Task<HttpStatusCode> SendEmail(EmailMessage emailMessage)
    {
        if (string.IsNullOrEmpty(emailMessage.ToEmail))
            throw new ApplicationException("AwsSesProvider:: SendEmail, ToEmail cannot be null or empty. No email has been sent.");

        var result = await SendAwsSesEmail(emailMessage);

        return result.HttpStatusCode;
    }

    private async Task<SendRawEmailResponse> SendAwsSesEmail(EmailMessage emailMessage)
    {
        var emailBuilder = new EmailBuilder();

        var sendRequest = new SendRawEmailRequest
        {
            RawMessage = new RawMessage(emailBuilder.BuildMessageToStream(emailMessage))
        };

        try
        {
            return await emailService.SendRawEmailAsync(sendRequest);
        }
        catch (Exception ex)
        {
            throw new Exception($"AwsSesProvider:: SendEmail, An error has occured while attempting to send an email to Amazon Ses. \n{ex.Message}");
        }
    }
}