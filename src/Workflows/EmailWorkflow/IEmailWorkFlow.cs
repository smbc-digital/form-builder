namespace form_builder.Workflows.EmailWorkflow
{
    public interface IEmailWorkflow
    {
        Task<string> Submit(string form);
    }
}
