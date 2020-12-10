namespace form_builder.TagParser
{
    public interface IFormatter
    {
        string FormatterrName { get; }
        string Parse(string value);
    }
}