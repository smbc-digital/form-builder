using form_builder.Extensions;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.TemplatedEmailProvider;
using Newtonsoft.Json;

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

        public Task ProcessTemplatedEmail(List<IAction> actions, string form)
        {
            ISession browserSessionId = _sessionHelper.GetSession();
            string formSessionId = $"{form}::{browserSessionId.Id}";

            if (string.IsNullOrEmpty(formSessionId))
                throw new Exception("TemplatedEmailService::Process: Session has expired");

            var formData = _distributedCache.GetString(formSessionId);
            var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            foreach (var action in actions)
            {
                var templatedEmailProvider = _templatedEmailProviders.Get(action.Properties.EmailTemplateProvider);
                var convertedAnswers = formAnswers.AllAnswers.ToDictionary(x => x.QuestionId, x => x.Response);
                var personalisation = new Dictionary<string, dynamic>();

                if (action.Properties.IncludeCaseReference)
                    personalisation.Add("reference", formAnswers.CaseReference);

                if (action.Properties.Personalisation is not null && !convertedAnswers.Count.Equals(0))
                    action.Properties.Personalisation.ForEach(field => { personalisation.Add(field, convertedAnswers[field]); });

                action.ProcessTemplatedEmail(
                    _actionHelper,
                    templatedEmailProvider,
                    personalisation,
                    formAnswers);
            }
            return null;
        }
    }
}
