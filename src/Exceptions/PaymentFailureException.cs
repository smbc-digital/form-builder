namespace form_builder.Exceptions
{
    public class PaymentFailureException : Exception
    {
        public PaymentFailureException(string message)
            : base(message)
        {
        }
    }
}