using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers;

public class ItalicTagParser : TagParser, ITagParser
{
    public ItalicTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

    public Regex Regex => new Regex("(?<={{)ITALIC::.*?(?=}})", RegexOptions.Compiled);

    public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
    {
        page.Elements.Select((element) =>
        {
            if (!string.IsNullOrEmpty(element.Properties?.Text))
                element.Properties.Text = Parse(element.Properties.Text, Regex);

            if (!string.IsNullOrEmpty(element.Properties?.Hint))
                element.Properties.Hint = Parse(element.Properties.Hint, Regex);

            return element;
        }).ToList();

        return await Task.FromResult(page);
    }

    public string ParseString(string content, FormAnswers formAnswers)
    {
        var updatedContent = content;
        var matches = Regex.Matches(content);

        foreach (Match match in matches)
        {
            var value = $"{{{{{match.Value}}}}}";

            try
            {
                var replacedContent = Parse(value, Regex);
                updatedContent = updatedContent.Replace(value, replacedContent);
            }
            catch (Exception)
            {
                updatedContent = updatedContent.Replace(value, string.Empty);
            }
        }

        return updatedContent;
    }
}
