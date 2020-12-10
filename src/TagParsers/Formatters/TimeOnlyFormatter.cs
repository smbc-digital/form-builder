using form_builder.Utils.Extesions;

namespace form_builder.TagParser
{
    public class TimeOnlyFormatter : IFormatter
    {
        public string FormatterrName { get => "time-only"; }
        public string Parse(string value)
        {
            var dateTime = System.DateTime.Parse(value);
            return dateTime.ToTimeFormat();
        }
    }
}