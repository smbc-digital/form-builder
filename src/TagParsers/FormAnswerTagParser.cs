using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.TagParser
{
    public class FormAnswerTagParser : TagParser, ITagParser
    {        
        public FormAnswerTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

        public Regex Regex => new Regex("(?<={{)QUESTION:.*?(?=}})", RegexOptions.Compiled);

        public Page Parse(Page page, FormAnswers formAnswers)
        {
            var answersDictionary = formAnswers.Pages?.SelectMany(x => x.Answers).ToDictionary(x => x.QuestionId, x => x.Response);

            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties?.Text))
                    element.Properties.Text = Parse(element.Properties.Text, answersDictionary, Regex);

                return element;
            }).ToList();

            return page;
        }
    }
}