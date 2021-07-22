using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class FormDataTagParser : TagParser, ITagParser
    {
        public FormDataTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

        public Regex Regex => new Regex("(?<={{)FORMDATA:.*?(?=}})", RegexOptions.Compiled);

        public Page Parse(Page page, FormAnswers formAnswers)
        {
            var answersDictionary = formAnswers.AdditionalFormData;

            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties?.Text))
                    element.Properties.Text = Parse(element.Properties.Text, answersDictionary, Regex);

                return element;
            }).ToList();

            return page;
        }

        public string ParseString(string content, FormAnswers formAnswers) => Parse(content, formAnswers.AdditionalFormData, Regex);
    }
}