using Notify.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.EmailTemplateProvider
{
    public class NotifyEmailTemplateProvider : IEmailTemplateProvider
    {
        public string ProviderName { get => "SMBC"; }

        public Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation)
        {
            throw new NotImplementedException();
        }
    }
}
