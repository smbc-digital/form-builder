using form_builder.Models.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Services.TemplatedEmailService
{
    public interface ITemplatedEmailService
    {
        Task ProcessTemplatedEmail(List<IAction> actions);
    }
}
