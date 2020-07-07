using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Services.ActionService
{
    public interface IActionService
    {
        Task Process();
    }

    public class ActionService : IActionService
    {
        public async Task Process()
        {
            // Do stuff here
        }
    }
}
