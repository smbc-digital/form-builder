namespace form_builder.Exceptions
{
    public class BookingNoAvailabilityException : Exception
    {
        public BookingNoAvailabilityException(string message)
            : base(message)
        {
        }
    }
}