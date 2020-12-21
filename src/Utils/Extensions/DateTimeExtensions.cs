using System;

namespace form_builder.Utils.Extesions
{
    public static class DateTimeExtensions
    {
        public static DateTime LastDayOfTheMonth(this DateTime today)
        {
            var totalDays = DateTime.DaysInMonth(today.Date.Year, today.Date.Month);
            return new DateTime(today.Date.Year, today.Date.Month, totalDays, 23, 59, 59);
        }

        public static string ToTimeFormat(this DateTime value) => value.ToString(value.Minute > 0 ? "h:mmtt" :"htt").ToLower();
        public static string ToFullDateFormat(this DateTime value) => value.ToString("dddd d MMMM yyyy");

        public static int PreviousDaysInMonth(this DateTime value) {
            switch (value.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return 6;
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                default:
                    return 0;
            }
        }
    }
}