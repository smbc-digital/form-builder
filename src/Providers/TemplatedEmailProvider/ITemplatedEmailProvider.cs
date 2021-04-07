using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.TemplatedEmailProvider
{
    public interface ITemplatedEmailProvider
    {
        public string ProviderName { get => "SMBC"; }

        Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation);
    }
}
