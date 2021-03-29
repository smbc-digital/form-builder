using System;

namespace form_builder.Utils.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToTimeFormat(this TimeSpan value) => new DateTime().Add(value).ToString(value.Minutes > 0 ? "h:mmtt" : "htt").ToLower();
    }
}