namespace form_builder.ViewModels
{
    public class FakePaymentViewModel : FormBuilderViewModel
    {
        public string Reference { get; set; }
        public string Amount { get; set; }
        public string PaymentEndpointBaseUrl { get; set; }

        public FakePaymentViewModel()
        {
        }

        public FakePaymentViewModel(string url, string reference, string amount, string formName)
        {
            PaymentEndpointBaseUrl = url;
            Reference = reference;
            Amount = amount;
            FormName = formName;
        }
    }
}