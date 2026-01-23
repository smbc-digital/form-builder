using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers;

public class ListTagParser : TagParser, ITagParser
{
    public ListTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

    public Regex Regex => new Regex("(?<={{)([UO]LIST)::.*?(?=}})", RegexOptions.Compiled);

    public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
    {
        page.Elements.Select((element) =>
        {
            if (element.Properties is null)
                return element;

            if (!string.IsNullOrEmpty(element.Properties?.IAG))
                element.Properties.IAG = ParseList(element.Properties.IAG, Regex, false);

            if (!string.IsNullOrEmpty(element.Properties?.Hint))
                element.Properties.Hint = ParseList(element.Properties.Hint, Regex, true);

            if (element.Properties.ListItems.Any())
            {
                for (int item = 0; item < element.Properties.ListItems.Count; item++)
                {
                    element.Properties.ListItems[item] = ParseList(element.Properties.ListItems[item], Regex, true);
                }
            }

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
                var replacedContent = ParseList(value, Regex, false);
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
