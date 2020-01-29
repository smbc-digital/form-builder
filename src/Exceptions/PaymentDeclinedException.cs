using System;

namespace form_builder.Exceptions
{
    public class PaymentDeclinedException : Exception
    {
        public PaymentDeclinedException(string message)
            : base(message)
        {
        }
    }
}