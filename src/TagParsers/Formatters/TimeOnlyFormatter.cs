using form_builder.Utils.Extensions;

namespace form_builder.TagParsers.Formatters
{
    public class TimeOnlyFormatter : IFormatter
    {
        public string FormatterName => "time-only";

        public string Parse(string value)
        {
            var dateTime = System.DateTime.Parse(value);
            return dateTime.ToTimeFormat();
        }
    }
}