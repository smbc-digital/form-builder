namespace form_builder.TagParsers;

public class FormAnswerOptionalTagParser(IEnumerable<IFormatter> formatters) : TagParser(formatters), ITagParser
{
    private readonly bool _allowOptional = true;

    public Regex Regex => new("(?<={{)QUESTIONOPT:.*?(?=}})", RegexOptions.Compiled);

    public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
    {
        var answersDictionary = formAnswers.Pages?.SelectMany(x => x.Answers).ToDictionary(x => x.QuestionId, x => x.Response);

        var leadingParagraphRegexIsMatch = !string.IsNullOrEmpty(page.LeadingParagraph) && Regex.IsMatch(page.LeadingParagraph);

        if (leadingParagraphRegexIsMatch)
            page.LeadingParagraph = Parse(page.LeadingParagraph, answersDictionary, Regex, _allowOptional);

        page.Elements.Select(element =>
        {
            if (!string.IsNullOrEmpty(element.Properties?.Text))
                element.Properties.Text = Parse(element.Properties.Text, answersDictionary, Regex, _allowOptional);

            if (!string.IsNullOrEmpty(element.Properties?.Hint))
                element.Properties.Hint = Parse(element.Properties.Hint, answersDictionary, Regex, _allowOptional);

            if (!string.IsNullOrEmpty(element.Properties?.LimitNextAvailableFromDate))
                element.Properties.LimitNextAvailableFromDate = Parse(element.Properties.LimitNextAvailableFromDate, answersDictionary, Regex, _allowOptional);

            if (element.Properties.ListItems.Any())
            {
                for (int item = 0; item < element.Properties.ListItems.Count; item++)
                {
                    element.Properties.ListItems[item] = Parse(element.Properties.ListItems[item], answersDictionary, Regex, _allowOptional);
                }
            }

            return element;
        }).ToList();

        return await Task.FromResult(page);
    }

    public string ParseString(string content, FormAnswers formAnswers)
    {
        var answersDictionary = formAnswers.Pages?.SelectMany(x => x.Answers).ToDictionary(x => x.QuestionId, x => x.Response);
        var updatedContent = content;
        var matches = Regex.Matches(content);

        foreach (Match match in matches)
        {
            var questionId = $"{{{{{match.Value}}}}}";

            try
            {
                var replacedContent = Parse(questionId, answersDictionary, Regex, _allowOptional);
                updatedContent = updatedContent.Replace(questionId, replacedContent);
            }
            catch (Exception)
            {
                updatedContent = updatedContent.Replace(questionId, string.Empty);
            }
        }

        return updatedContent;
    }
}