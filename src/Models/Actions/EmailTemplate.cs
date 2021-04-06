using form_builder.Enum;

namespace form_builder.Models.Actions
{
    public class EmailTemplate : Action
    {
        public EmailTemplate()
        {
            Type = EActionType.EmailTemplate;
        }
    }
}
