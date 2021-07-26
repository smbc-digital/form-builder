using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public async Task<Page> Parse(Page page, FormAnswers formAnswers)
        {
            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties?.Text))
                    element.Properties.Text = Parse(element.Properties.Text, Regex, _htmlContent, FormatContent);

                return element;
            }).ToList();

            return page;
        }

        public string ParseString(string content, FormAnswers formAnswers) => Parse(content, Regex, _htmlContent, FormatContent);

        public string FormatContent(string[] values) => string.Format(_htmlContent, values[0], values[1]);
    }
}