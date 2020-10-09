using form_builder.Enum;

namespace form_builder.Models.Actions
{
    public class Validate : Action
    {
        public Validate()
        {
            Type = EActionType.Validate;
        }
    }
}
