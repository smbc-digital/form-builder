namespace form_builder.TagParsers.Formatters
{
    public interface IFormatter
    {
        string FormatterName { get; }
        string Parse(string value);
    }
}