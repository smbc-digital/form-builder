namespace form_builder.Workflows.PaymentWorkflow
{
    public interface IPaymentWorkflow
    {
        Task<string> Submit(string form, string path);
    }
}
