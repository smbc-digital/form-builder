using System;

namespace form_builder.Exceptions
{
    public class PaymentCallbackException : Exception
    {
        public PaymentCallbackException(string message)
            : base(message)
        {
        }
    }
}
