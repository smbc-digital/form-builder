using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers;

public class ImageTagParser : TagParser, ITagParser, ISimpleTagParser
{
    public ImageTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

    public Regex Regex => new Regex("(?<={{)IMAGE::.*?(?=}})", RegexOptions.Compiled);
    public string _htmlContent => "<img class='form-builder-img' src='{0}' alt='{1}'>";

    public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
    {
        page.Elements.Select((element) =>
        {
            if (!string.IsNullOrEmpty(element.Properties?.Text))
                element.Properties.Text = Parse(element.Properties.Text, Regex, _htmlContent, FormatContent, "::");

            if (!string.IsNullOrEmpty(element.Properties?.Hint))
                element.Properties.Hint = Parse(element.Properties.Hint, Regex, _htmlContent, FormatContent, "::");

            return element;
        }).ToList();

        return await Task.FromResult(page);
    }

    public string ParseString(string content, FormAnswers formAnswers) => Parse(content, Regex, _htmlContent, FormatContent, "::");

    public string FormatContent(string[] values) => string.Format(_htmlContent, values[0], values[1]);
}
