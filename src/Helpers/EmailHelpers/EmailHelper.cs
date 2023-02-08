using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Helpers.Session;
using form_builder.Providers.Transforms.EmailConfiguration;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Helpers.EmailHelpers
{
    public class EmailHelper : IEmailHelper
    {

        private readonly IEmailConfigurationTransformDataProvider _emailConfigProvider;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMappingService _mappingService;

        public EmailHelper(
           ISessionHelper sessionHelper,
           IMappingService mappingService,
           IEmailConfigurationTransformDataProvider emailConfigProvider)
        {
            _sessionHelper = sessionHelper;
            _mappingService = mappingService;
            _emailConfigProvider = emailConfigProvider;
        }

        public async Task<EmailConfiguration> GetEmailInformation(string form)
         {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var mappingEntity = await _mappingService.Map(sessionGuid, form);
            if (mappingEntity is null)
                throw new Exception($"{nameof(EmailHelper)}::{nameof(GetEmailInformation)}: No mapping entity found for {form}");

            var emailConfigList = await _emailConfigProvider.Get<List<EmailConfiguration>>();
            var formEmailConfig = emailConfigList.FirstOrDefault(_ => _.FormName.Any(_ => _.Equals(form)));

            if (formEmailConfig is null)
                throw new Exception($"{nameof(EmailHelper)}::{nameof(GetEmailInformation)}: No email information found for {form}");
            
            return formEmailConfig;
        }
    }
}
