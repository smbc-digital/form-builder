using form_builder.Utils.Extensions;

namespace form_builder.TagParsers.Formatters
{
    public class FullDateFormatter : IFormatter
    {
        public string FormatterName => "full-date";

        public string Parse(string value)
        {
            var dateTime = System.DateTime.Parse(value);
            return dateTime.ToFullDateFormat();
        }
    }
}