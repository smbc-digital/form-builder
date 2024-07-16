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
        public UserEmail() => Type = EActionType.UserEmail;

        public override async Task Process(IActionHelper actionHelper, IEmailProvider emailProvider, FormAnswers formAnswers)
        {
            var emails = string.Empty;
            try
            {
                emails = actionHelper.GetEmailToAddresses(this, formAnswers);
            }
            catch (Exception)
            {
                // no email address submitted (optional) to the form so form actions cannot complete this action. 
            }

            if (emails != string.Empty)
            {
                await emailProvider
                    .SendEmail(
                        new EmailMessage(
                            Properties.Subject,
                            actionHelper.GetEmailContent(this, formAnswers),
                            Properties.From,
                            actionHelper.GetEmailToAddresses(this, formAnswers)));
            }
        }
    }
}