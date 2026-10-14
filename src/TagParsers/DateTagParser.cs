namespace form_builder.TagParsers;

public class DateTagParser(IEnumerable<IFormatter> formatters) : TagParser(formatters), ITagParser
{
    public Regex Regex => new(@"\{\{DATE::[^}]+\}\}", RegexOptions.Compiled);

    public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
    {
        if (!string.IsNullOrEmpty(page.LeadingParagraph))
            page.LeadingParagraph = ParseDate(page.LeadingParagraph, Regex);

        page.Elements = page.Elements.Select(element =>
        {
            if (!string.IsNullOrEmpty(element.Properties?.Text))
                element.Properties.Text = ParseDate(element.Properties.Text, Regex);

            if (!string.IsNullOrEmpty(element.Properties?.Hint))
                element.Properties.Hint = ParseDate(element.Properties.Hint, Regex);

            if (!string.IsNullOrEmpty(element.Properties?.Label))
                element.Properties.Label = ParseDate(element.Properties.Label, Regex);

            return element;
        }).ToList();

        return page;
    }

    public string ParseString(string content, FormAnswers formAnswers) =>
        ParseDate(content, Regex);
}