namespace form_builder.Workflows.PaymentWorkflow;

public class PaymentWorkflow(IPayService payService,
    ISubmitService submitService,
    IMappingService mappingService,
    ISessionHelper sessionHelper)
    : IPaymentWorkflow
{
    public async Task<string> Submit(string form, string path)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException("A Session GUID was not provided.");

        string formSessionId = $"{form}::{browserSessionId}";

        await submitService.PreProcessSubmission(form, formSessionId);

        var data = await mappingService.Map(formSessionId, form, null, null);
        var paymentReference = await submitService.PaymentSubmission(data, form, formSessionId);

        return await payService.ProcessPayment(data, form, path, paymentReference, formSessionId);
    }
}