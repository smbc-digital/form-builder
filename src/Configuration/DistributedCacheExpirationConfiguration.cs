namespace form_builder.Configuration
{
    public class DistributedCacheExpirationConfiguration
    {
        public const string ConfigValue = "DistributedCacheExpiration";
        public int UserData { get; set; }
        public int PaymentConfiguration { get; set; }
        public int FormJson { get; set; }
        public int FileUpload { get; set; }
        public int Document { get; set; }
        public int Index { get; set; }
        public int Booking { get; set; }
        public int BookingNoAppointmentsAvailable { get; set; }
    }
}