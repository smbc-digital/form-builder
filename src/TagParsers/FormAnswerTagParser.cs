using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class FormAnswerTagParser : TagParser, ITagParser
    {

        public FormAnswerTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

        public Regex Regex => new Regex("(?<={{)QUESTION:.*?(?=}})", RegexOptions.Compiled);

        public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
        {
            var answersDictionary = formAnswers.Pages?.SelectMany(x => x.Answers).ToDictionary(x => x.QuestionId, x => x.Response);

            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties?.Text))
                    element.Properties.Text = Parse(element.Properties.Text, answersDictionary, Regex);

                if (!string.IsNullOrEmpty(element.Properties?.Hint))
                    element.Properties.Hint = Parse(element.Properties.Hint, answersDictionary, Regex);

                if (!string.IsNullOrEmpty(element.Properties?.LimitNextAvailableFromDate))
                    element.Properties.LimitNextAvailableFromDate = Parse(element.Properties.LimitNextAvailableFromDate, answersDictionary, Regex);

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
                    var replacedContent = Parse(questionId, answersDictionary, Regex);
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
}