using form_builder.ContentFactory.SuccessPageFactory;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Providers.EmailProvider;
using form_builder.Services.MappingService;
using Newtonsoft.Json;

namespace form_builder.Models.Actions
{
    public class UserEmail : Action
    {
        private readonly ILogger<UserEmail> _logger;
        public UserEmail(
            ILogger<UserEmail> logger
            )
        {
            Type = EActionType.UserEmail;
            _logger = logger;
        }

        public override async Task Process(IActionHelper actionHelper, IEmailProvider emailProvider, FormAnswers formAnswers)
        {
            try
            {
                await emailProvider
                    .SendEmail(
                        new EmailMessage(
                            Properties.Subject,
                            actionHelper.GetEmailContent(this, formAnswers),
                            Properties.From,
                            actionHelper.GetEmailToAddresses(this, formAnswers)));

            }
            catch (Exception)
            {
                //_logger.LogInformation($"{nameof(MappingService)}::{nameof(UserEmail)}: - Failed Email Send action - {JsonConvert.SerializeObject(formAnswers)}");
                _logger.LogInformation($"UserEmail: - Failed Email Send action ");
            }
        }
    }
}