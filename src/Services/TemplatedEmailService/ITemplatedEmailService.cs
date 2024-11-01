using form_builder.Models.Actions;

namespace form_builder.Services.TemplatedEmailService
{
    public interface ITemplatedEmailService
    {
        Task ProcessTemplatedEmail(List<IAction> actions, string form);
    }
}
