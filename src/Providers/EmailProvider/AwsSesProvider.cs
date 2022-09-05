using System.Net;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using form_builder.Builders.Email;
using form_builder.Models;

namespace form_builder.Providers.EmailProvider
{
    public class AwsSesProvider : IEmailProvider
    {
        private readonly IAmazonSimpleEmailService _emailService;
        public AwsSesProvider(IAmazonSimpleEmailService emailService) => _emailService = emailService;

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
                return await _emailService.SendRawEmailAsync(sendRequest);
            }
            catch (Exception ex)
            {
                throw new Exception($"AwsSesProvider:: SendEmail, An error has occured while attempting to send an email to Amazon Ses. \n{ex.Message}");
            }
        }
    }
}