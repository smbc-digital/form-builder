using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Factories.TagParser
{
    public class FormAnswerTagParser : ITagParser
    {
        public Regex Regex => new Regex("(?<={{)QUESTION:.*?(?=}})", RegexOptions.Compiled);

        public Page Parse(Page page, FormAnswers formAnswers)
        {
            var answersDictionary = formAnswers.Pages?.SelectMany(_ => _.Answers).ToDictionary(x => x.QuestionId, x => x.Response);

            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties.Text))
                {
                    return Parse(element, answersDictionary);
                }
                return element;
            }).ToList();

            return page;
        }

        private IElement Parse(IElement element, Dictionary<string, object> answersDictionary)
        {
            var matches = Regex.Matches(element.Properties.Text);

            if (matches.Any())
            {
                foreach (Match match in matches)
                {
                    var splitMatch = match.Value.Split(":");
                    var questionId = splitMatch[1];

                    if (string.IsNullOrEmpty(questionId))
                        continue;

                    var value = (string)answersDictionary[questionId];

                    if (string.IsNullOrEmpty(value))
                        continue;

                    var replacementText = new StringBuilder(element.Properties.Text);
                    replacementText.Remove(match.Index - 2, match.Length + 4);
                    replacementText.Insert(match.Index - 2, value);
                    element.Properties.Text = replacementText.ToString();
                }
            }

            return element;
        }
    }
}