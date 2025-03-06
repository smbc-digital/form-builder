using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class CaseReferenceTagParser : TagParser, ITagParser
    {
        public CaseReferenceTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

        public Regex Regex => new Regex("(?<={{)CASEREFERENCE.*?(?=}})", RegexOptions.Compiled);

        public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
        {
            var leadingParagraphRegexIsMatch = !string.IsNullOrEmpty(page.LeadingParagraph) && Regex.IsMatch(page.LeadingParagraph);
            var pageHasElementsMatchingRegex = page.Elements.Any(_ => _.Properties.Text is not null && Regex.IsMatch(_.Properties.Text));

            if (leadingParagraphRegexIsMatch || pageHasElementsMatchingRegex)
            {
                if (leadingParagraphRegexIsMatch)
                {
                    page.LeadingParagraph = Parse(page.LeadingParagraph, formAnswers.CaseReference, Regex);
                }

                if (pageHasElementsMatchingRegex)
                {
                    page.Elements.Select((element) =>
                    {
                        if (!string.IsNullOrEmpty(element.Properties?.Text))
                            element.Properties.Text = Parse(element.Properties.Text, formAnswers.CaseReference, Regex);

                        return element;
                    }).ToList();
                }
            }

            return await Task.FromResult(page);
        }

        public string ParseString(string content, FormAnswers formAnswers) => Regex.IsMatch(content) ? Parse(content, formAnswers.CaseReference, Regex) : content;
    }
}
