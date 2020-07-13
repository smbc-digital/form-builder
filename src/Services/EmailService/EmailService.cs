﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.EmailProvider;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;

namespace form_builder.Services.EmailService
{
    public interface IEmailService
    {
        Task Process(List<IAction> actions, FormSchema baseForm);
    }

    public class EmailService : IEmailService
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IEmailProvider _emailProvider;
        private readonly IActionHelper _actionHelper;

        public EmailService(ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache, IEnumerable<IEmailProvider> emailProviders, IActionHelper actionHelper)
        {
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
            _emailProvider = emailProviders.First();
            _actionHelper = actionHelper;
        }

        public async Task Process(List<IAction> actions, FormSchema baseForm)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new Exception("EmailService::Process: Session has expired");

            var formData = _distributedCache.GetString(sessionGuid);
            var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            foreach (var action in actions)
            {
                await action.Process(_actionHelper, _emailProvider, formAnswers);
            }
        }
    }
}