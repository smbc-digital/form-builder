namespace form_builder.Exceptions
{
    public class BookingCannotBeCancelledException : Exception
    {
        public BookingCannotBeCancelledException(string message)
            : base(message)
        {
        }
    }
}