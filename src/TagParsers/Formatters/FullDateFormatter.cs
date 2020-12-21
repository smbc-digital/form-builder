using form_builder.Utils.Extesions;

namespace form_builder.TagParser
{
    public class FullDateFormatter : IFormatter
    {
        public string FormatterrName { get => "full-date"; }
        public string Parse(string value)
        {
            var dateTime = System.DateTime.Parse(value);
            return dateTime.ToFullDateFormat();
        }
    }
}