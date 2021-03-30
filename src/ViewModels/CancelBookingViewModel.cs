using form_builder.Utils.Extensions;
using System;

namespace form_builder.ViewModels
{
    public class CancelBookingViewModel : FormBuilderViewModel
    {
        public string FormattedTime => IsFullday ? $"between {StartTime.ToTimeFormat()} and {EndTime.ToTimeFormat()}" : $"{StartTime.ToTimeFormat()} to {EndTime.ToTimeFormat()}";
        public string FormattedDate => BookingDate.ToFullDateFormat();
        public string Hash { get; set; }
        public string BaseURL { get; set; }
        public Guid Id { get; set; }
        public bool Cancellable { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsFullday { get; set; }
    }
}
