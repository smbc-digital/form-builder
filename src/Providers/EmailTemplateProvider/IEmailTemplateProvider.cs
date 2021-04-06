using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.EmailTemplateProvider
{
    public interface IEmailTemplateProvider
    {
        string ProviderName { get; }

        Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation);
    }
}
