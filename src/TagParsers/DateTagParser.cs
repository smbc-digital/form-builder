using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using System.Text.RegularExpressions;

public class DateTagParser : TagParser, ITagParser
{
    public DateTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

    public Regex Regex => new Regex(@"\{\{DATE::[^}]+\}\}", RegexOptions.Compiled);

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