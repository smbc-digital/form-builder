using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Providers.EmailProvider;

namespace form_builder.Models.Actions
{
    public class UserEmail : Action
    {
        public UserEmail() => Type = EActionType.UserEmail;

        public override async Task Process(IActionHelper actionHelper, IEmailProvider emailProvider, FormAnswers formAnswers)
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