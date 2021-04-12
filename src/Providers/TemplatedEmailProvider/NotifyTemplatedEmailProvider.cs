using System;
using System.Collections.Generic;
using System.Net;
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

        public Task SendEmailAsync(string emailAddress, string templateId,
            Dictionary<string, dynamic> personalisation)
        {
            var result = _notifyClient.SendEmailAsync(emailAddress, templateId, personalisation);
            if (!result.IsCompletedSuccessfully)
                _logger.LogError($"Notify email provider :: email failed to send using template id {templateId}: {result.Exception.Message}");

            return null;
        }
    }
}
