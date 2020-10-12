using form_builder.Enum;

namespace form_builder.Models.Actions
{
    public class RetrieveExternalData : Action
    {
        public RetrieveExternalData()
        {
            Type = EActionType.RetrieveExternalData;
        }
    }
}