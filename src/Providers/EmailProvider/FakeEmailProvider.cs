using System.Net;
using form_builder.Models;

namespace form_builder.Providers.EmailProvider
{
    public class FakeEmailProvider : IEmailProvider
    {
        private readonly ILogger<FakeEmailProvider> _logger;
        public FakeEmailProvider(ILogger<FakeEmailProvider> logger) => _logger = logger;

        public async Task<HttpStatusCode> SendEmail(EmailMessage emailMessage)
        {
            _logger.LogInformation($"FakeEmailProvider: Sent fake email to {emailMessage.ToEmail}");
            return await Task.FromResult(HttpStatusCode.OK);
        }
    }
}