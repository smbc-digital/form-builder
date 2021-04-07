using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Providers.TemplatedEmailProvider;

namespace form_builder.Models.Actions
{
    public class TemplatedEmail : Action
    {
        public TemplatedEmail()
        {
            Type = EActionType.TemplatedEmail;
        }

        public override async Task ProcessTemplatedEmail(IActionHelper actionHelper, ITemplatedEmailProvider templatedEmailProvider, Dictionary<string, dynamic> personalisation, FormAnswers formAnswers)
        {
            await templatedEmailProvider
                .SendEmailAsync(actionHelper.GetEmailToAddresses(this, formAnswers), Properties.TemplateId, personalisation);
        }
    }
}
