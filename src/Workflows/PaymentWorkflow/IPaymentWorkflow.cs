using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Workflows.PaymentWorkflow
{
    public interface IPaymentWorkflow
    {
        Task<string> Submit(string form, string path);
    }
}
