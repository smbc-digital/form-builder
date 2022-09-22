using form_builder.Constants;
using form_builder.Enum;
using form_builder.Utils.Extensions;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;

namespace form_builder.Models.Time
{
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
        public string GetTimeInputHint(int position) => $"{TimeQuestionId}-{position}-{TimeOfDay}-{Date.Day}-hint";
        public string ToFullDateFormat(TimeSpan time) => Value(time).ToFullDateWithTimeFormat();
        public string NoJSId => $"{TimeQuestionId}-{BookingConstants.APPOINTMENT_FULL_TIME_OF_DAY_SUFFIX}";
        public string BookingType { get; set; }
        public string GetClassNameForDivsPosition(int value)
        {
            switch (value % 3)
            {
                case 0:
                    return "smbc-time__time__label--left";
                case 1:
                    return "smbc-time__time__label--centre";
                default:
                    return "smbc-time__time__label--right";
            }
        }
    }
}