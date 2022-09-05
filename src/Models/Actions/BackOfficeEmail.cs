using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Providers.EmailProvider;

namespace form_builder.Models.Actions
{
    public class BackOfficeEmail : Action
    {
        public BackOfficeEmail()
        {
            Type = EActionType.RetrieveExternalData;
        }

        public override async Task Process(IActionHelper actionHelper, IEmailProvider emailProvider, FormAnswers formAnswers)
        {
            var message = new EmailMessage(
                              this.Properties.Subject,
                              this.Properties.Content,
                              this.Properties.From,
                              actionHelper.GetEmailToAddresses(this, formAnswers));

            await emailProvider.SendEmail(message);
        }
    }
}