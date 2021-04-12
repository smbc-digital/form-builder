using System.Collections.Generic;
using System.Linq;
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
            var emailAddressList = actionHelper.GetEmailToAddresses(this, formAnswers).Split(',').ToList();

            foreach (var emailAddress in emailAddressList)
            {
                await templatedEmailProvider
                    .SendEmailAsync(emailAddress, Properties.TemplateId, personalisation);
            }
        }
    }
}
