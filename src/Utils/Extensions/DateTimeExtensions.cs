using System;

namespace form_builder.Utils.Extesions
{
    public static class DateTimeExtensions
    {
        public static DateTime LastDayOfTheMonth(this DateTime today)
        {
            var totalDays = DateTime.DaysInMonth(today.Date.Year, today.Date.Month);
            var lastDayOfMonth = new DateTime(today.Date.Year, today.Date.Month, totalDays, 23, 59, 59);
            return lastDayOfMonth;
        }


        public static bool IsWeekend(this DateTime value) =>
            value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday;

    }
}