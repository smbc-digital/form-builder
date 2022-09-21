using form_builder.Enum;

namespace form_builder.Models.Time
{
    public class TimeAvailability
    {
        public DateTime Date { get; set; }
        public TimePeriod MorningAppointments { get; set; }
        public TimePeriod AfternoonAppointments { get; set; }
        public string CurrentSelectedValue { get; set; }
        public string TimeSelectionIdForConditionalReveal => $"{DateQuestionId}-{Date.Day}-conditional";
        public string TimeQuestionId { get; set; }
        public string DateQuestionId { get; set; }
        public ETimePeriod TimePeriodCurrentlySelected { get; set; }
        public ETimePeriod SelectedTimePeriod { get; set; }
    }
}