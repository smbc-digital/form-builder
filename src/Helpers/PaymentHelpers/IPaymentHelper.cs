namespace form_builder.Helpers.PaymentHelpers;

public interface IPaymentHelper
{
    Task<PaymentInformation> GetFormPaymentInformation(string formName, FormAnswers formAnswers, FormSchema baseForm);
}