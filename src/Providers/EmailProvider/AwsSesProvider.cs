using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using form_builder.Builders.Email;
using form_builder.Models;
using Microsoft.Extensions.Logging;

namespace form_builder.Providers.EmailProvider
{
    public class AwsSesProvider : IEmailProvider
    {
        private readonly IAmazonSimpleEmailService _emailService;
        private readonly ILogger<AwsSesProvider> _logger;

        public AwsSesProvider(IAmazonSimpleEmailService emailService, ILogger<AwsSesProvider> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<HttpStatusCode> SendAwsSesEmail(EmailMessage emailMessage)
        {
            if (string.IsNullOrEmpty(emailMessage.ToEmail))
            {
                _logger.LogError("AwsSesProvider:: SendAwsSesEmail, ToEmail cannot be null or empty. No email has been sent.");
                return HttpStatusCode.InternalServerError;
            }

            var result = await SendEmail(emailMessage);

            return result.HttpStatusCode;
        }

        private async Task<SendRawEmailResponse> SendEmail(EmailMessage emailMessage)
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
                _logger.LogError($"AwsSesProvider:: SendEmail, An error occured trying to send an email to Amazon Ses. \n{ex.Message}");
                return new SendRawEmailResponse
                {
                    HttpStatusCode = HttpStatusCode.BadRequest
                };
            }

        }
    }
}