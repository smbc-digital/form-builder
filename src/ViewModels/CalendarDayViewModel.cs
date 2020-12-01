using form_builder.Models.Booking;

namespace form_builder.ViewModels
{
    public class CalendarDayViewModel
    {
        public string QuestionId { get; set; }
        public string CurrentValue { get; set; }
        public CalendarDay Day { get; set; }
    }
}