using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.ViewModels;
using StockportGovUK.NetStandard.Models.Booking.Response;

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

    public class TimePeriod
    {
        public ETimePeriod TimeOfDay { get; set; }
        public List<AppointmentTime> Appointments { get; set; }
        public bool HasAppointments => Appointments.Any();
        public DateTime Date { get; set; }
        public string CurrentValue { get; set; }
        public string TimeQuestionId { get; set; }
        public string Id => $"{TimeQuestionId}-{Date.Day}-{TimeOfDay}";
        public DateTime Value(TimeSpan time) => Date.Add(time);
    }
}