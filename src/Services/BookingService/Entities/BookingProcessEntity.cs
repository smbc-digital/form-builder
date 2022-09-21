namespace form_builder.Services.BookingService.Entities
{
    public class BookingProcessEntity
    {
        public List<object> BookingInfo { get; set; }
        public bool BookingHasNoAvailableAppointments { get; set; }
    }
}
