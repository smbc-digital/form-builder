using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Notify.Interfaces;

namespace form_builder.Providers.TemplatedEmailProvider
{
    public class NotifyTemplatedEmailProvider : ITemplatedEmailProvider
    {
        private readonly IAsyncNotificationClient _notifyClient;
        private readonly ILogger<NotifyTemplatedEmailProvider> _logger;

        public NotifyTemplatedEmailProvider(IAsyncNotificationClient notifyClient, ILogger<NotifyTemplatedEmailProvider> logger)
        {
            _notifyClient = notifyClient;
            _logger = logger;
        }

        public string ProviderName { get => "Notify"; }

        public async Task SendEmailAsync(string emailAddress, string templateId,
            Dictionary<string, dynamic> personalisation)
        {
            try
            {
                await _notifyClient.SendEmailAsync(emailAddress, templateId, personalisation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"NotifyTemplatedEmailProvider::SendEmailAsync, email failed to send using template id {templateId}: {ex.Message}");
            }
        }
    }
}
