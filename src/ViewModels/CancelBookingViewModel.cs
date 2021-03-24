using form_builder.Models.Booking;
using form_builder.Utils.Extensions;

namespace form_builder.ViewModels
{
    public class CancelBookingViewModel : FormBuilderViewModel
    {
        public AppointmentInformation appointmentInformation { get; set; }
        public string FormattedTime => appointmentInformation.IsFullday ? $"between {appointmentInformation.StartTime.ToTimeFormat()} and {appointmentInformation.EndTime.ToTimeFormat()}" : $"{appointmentInformation.StartTime.ToTimeFormat()} to {appointmentInformation.EndTime.ToTimeFormat()}";
        public string FormattedCancelBookingDate => appointmentInformation.BookingDate.ToFullDateFormat();
    }
}
