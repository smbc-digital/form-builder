using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class LinkTagParser : TagParser, ITagParser, ISimpleTagParser
    {
        public LinkTagParser(IEnumerable<IFormatter> formatters) : base(formatters)
        {
        }

        public Regex Regex => new Regex("(?<={{)LINK:.*?(?=}})", RegexOptions.Compiled);
        public string _htmlContent => "<a class='govuk-link' rel='noreferrer noopener' target='_blank' href='https://{0}'>{1}</a>";

        public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
        {
            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties?.Text))
                    element.Properties.Text = Parse(element.Properties.Text, Regex, _htmlContent, FormatContent);

                if (element.Properties?.ListItems is not null)
                {
                    element.Properties.ListItems = element.Properties.ListItems
                        .Select(item => Parse(item, Regex, _htmlContent, FormatContent)).ToList();
                }

                return element;
            }).ToList();

            return await Task.FromResult(page);
        }

        public string ParseString(string content, FormAnswers formAnswers) => Parse(content, Regex, _htmlContent, FormatContent);

        public string FormatContent(string[] values) => string.Format(_htmlContent, values[0], values[1]);
    }
}