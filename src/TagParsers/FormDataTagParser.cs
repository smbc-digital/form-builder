using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class FormDataTagParser : TagParser, ITagParser
    {
        public FormDataTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

        public Regex Regex => new Regex("(?<={{)FORMDATA:.*?(?=}})", RegexOptions.Compiled);

        public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
        {
            var answersDictionary = formAnswers.AdditionalFormData;

            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties?.Text))
                    element.Properties.Text = Parse(element.Properties.Text, answersDictionary, Regex);

                if (!string.IsNullOrEmpty(element.Properties?.Hint))
                    element.Properties.Hint = Parse(element.Properties.Hint, answersDictionary, Regex);

                return element;
            }).ToList();

            return await Task.FromResult(page);
        }

        public string ParseString(string content, FormAnswers formAnswers) => Parse(content, formAnswers.AdditionalFormData, Regex);
    }
}