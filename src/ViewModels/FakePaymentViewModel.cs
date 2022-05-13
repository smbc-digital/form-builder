namespace form_builder.ViewModels
{
    public class FakePaymentViewModel : FormBuilderViewModel
    {
        public string Reference { get; set; }
        
        public string Amount { get; set; }
        public string PaymentEndpointBaseUrl { get; set; }
    }
}