using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models.Actions;

namespace form_builder.Services.EmailService
{
    public interface IEmailService
    {
        Task Process(List<IAction> actions);
    }
}
