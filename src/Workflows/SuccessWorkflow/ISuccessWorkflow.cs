namespace form_builder.Workflows.SuccessWorkflow;

public interface ISuccessWorkflow
{
    Task<SuccessPageEntity> Process(EBehaviourType behaviourType, string form);
}