namespace form_builder.Workflows.RedirectWorkflow
{
    public interface IRedirectWorkflow
    {
        Task<string> Submit(string form, string path);
    }
}
