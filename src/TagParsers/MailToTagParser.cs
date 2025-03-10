using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class MailToTagParser : TagParser, ITagParser, ISimpleTagParser
    {
        public MailToTagParser(IEnumerable<IFormatter> formatters) : base(formatters)
        {
        }

        public Regex Regex => new Regex("(?<={{)MAILTO:.+?[@].+?(?=}})", RegexOptions.Compiled);
        public string _htmlContent => "<a class='govuk-link' rel='noreferrer noopener' target='_blank' href='mailto:{0}'>{0}</a>";

        public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
        {
            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties?.Text))
                    element.Properties.Text = Parse(element.Properties.Text, Regex, _htmlContent, FormatContent);

                return element;
            }).ToList();

            return await Task.FromResult(page);
        }

        public string ParseString(string content, FormAnswers formAnswers) => Parse(content, Regex, _htmlContent, FormatContent);

        public string FormatContent(string[] values) => string.Format(_htmlContent, values[0]);
    }
}