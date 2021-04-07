using Notify.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace form_builder.Providers.TemplatedEmailProvider
{
    public class FakeTemplatedEmailProvider : ITemplatedEmailProvider
    {
        private readonly IAsyncNotificationClient _notifyClient;

        public FakeTemplatedEmailProvider(IAsyncNotificationClient notifyClient) => _notifyClient = notifyClient;

        public string ProviderName { get => "Fake"; }

        public async Task SendEmailAsync(
            string emailAddress,
            string templateId,
            Dictionary<string, dynamic> personalisation) => await Task.FromResult(HttpStatusCode.OK);
    }
}
