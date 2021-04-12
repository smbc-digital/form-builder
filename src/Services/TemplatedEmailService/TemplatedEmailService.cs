﻿using form_builder.Extensions;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.TemplatedEmailProvider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Services.TemplatedEmailService
{
    public class TemplatedEmailService : ITemplatedEmailService
    {
        private readonly IEnumerable<ITemplatedEmailProvider> _templatedEmailProviders;
        private readonly IActionHelper _actionHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;

        public TemplatedEmailService(
            IEnumerable<ITemplatedEmailProvider> templatedEmailProviders,
            IActionHelper actionHelper,
            ISessionHelper sessionHelper,
            IDistributedCacheWrapper distributedCache)
        {
            _templatedEmailProviders = templatedEmailProviders;
            _actionHelper = actionHelper;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
        }

        public Task ProcessTemplatedEmail(List<IAction> actions)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new Exception("TemplatedEmailService::Process: Session has expired");

            var formData = _distributedCache.GetString(sessionGuid);
            var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);


            foreach (var action in actions)
            {
                var templatedEmailProvider = _templatedEmailProviders.Get(action.Properties.EmailTemplateProvider);

                action.ProcessTemplatedEmail(
                    _actionHelper,
                    templatedEmailProvider,
                    new Dictionary<string, dynamic>(),
                    formAnswers);
            }

            return null;
        }
    }
}
