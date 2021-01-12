using System.Threading.Tasks;

namespace form_builder.Workflows.SubmitWorkflow
{
    public interface ISubmitWorkflow
    {
        Task<string> Submit(string form);
    }
}
