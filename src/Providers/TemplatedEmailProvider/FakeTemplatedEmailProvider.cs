using Notify.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.EmailTemplateProvider
{
    public class FakeTemplatedEmailProvider : ITemplatedEmailProvider
    {
        private readonly IAsyncNotificationClient _notifyClient;

        public FakeTemplatedEmailProvider(IAsyncNotificationClient notifyClient) => _notifyClient = notifyClient;

        public string ProviderName { get => "Fake"; }

        public async Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation) =>
                await _notifyClient.SendEmailAsync(emailAddress, templateId, personalisation);
    }
}
