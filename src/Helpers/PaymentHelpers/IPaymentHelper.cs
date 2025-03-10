using form_builder.Configuration;
using form_builder.Models;

namespace form_builder.Helpers.PaymentHelpers
{
    public interface IPaymentHelper
    {
        Task<PaymentInformation> GetFormPaymentInformation(string formName, FormAnswers formAnswers, FormSchema baseForm);
    }
}
