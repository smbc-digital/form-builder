namespace form_builder.Services.MappingService;

public interface IMappingService
{
    Task<MappingEntity> Map(string cacheKey, string form, FormAnswers convertedAnswers, FormSchema baseForm);
    Task<BookingRequest> MapBookingRequest(string cacheKey, IElement bookingElement, Dictionary<string, dynamic> viewModel, string form);
    void MapAppointmentId(AppointmentType appointmentType, FormAnswers answers);
}