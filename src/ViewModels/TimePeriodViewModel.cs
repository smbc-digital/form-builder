using form_builder.Enum;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;

namespace form_builder.ViewModels
{
    public class TimePeriodViewModel
    {
        public string QuestionId { get; set; }
        public DateTime Date { get; set; }
        public List<AppointmentTime> AppointmentTimes { get; set; }

        public ETimePeriod TimePeriod { get; set; }

        public string Id => $"{QuestionId}-{Date.Day}-{TimePeriod}";

        public DateTime Value(TimeSpan time)
        {
            return Date.Add(time);
        }
    }
}