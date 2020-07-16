using form_builder.Enum;
using Action = form_builder.Models.Actions.Action;

public class RetrieveExternalData : Action
{
    public RetrieveExternalData()
    {
        Type = EActionType.RetrieveExternalData;
    }
}