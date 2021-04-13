using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.TemplatedEmailProvider
{
    public class FakeTemplatedEmailProvider : ITemplatedEmailProvider
    {
        public string ProviderName { get => "Fake"; }

        public Task SendEmailAsync(
            string emailAddress,
            string templateId,
            Dictionary<string, dynamic> personalisation) => null;
    }
}
