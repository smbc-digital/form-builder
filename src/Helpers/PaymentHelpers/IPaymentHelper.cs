using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;

namespace form_builder.Helpers.PaymentHelpers
{
    public interface IPaymentHelper
    {
        Task<PaymentInformation> GetFormPaymentInformation(string form);
    }
}
