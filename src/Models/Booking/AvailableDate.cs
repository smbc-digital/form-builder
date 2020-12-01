using System;

namespace form_builder.Models.Booking
{
    public class CalendarDay
    {
            public DateTime Date { get; set; }
            public bool IsDisabled { get; set; }
            public bool IsNotWithinCurrentSelectedMonth {get;set;}
    }
}