using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using System.Text.RegularExpressions;

public class DateTagParser : TagParser, ITagParser
{
    public DateTagParser(IEnumerable<IFormatter> formatters) : base(formatters) { }

    public Regex Regex => new Regex(@"\{\{DATE:[^}]+\}\}", RegexOptions.Compiled);

    private string ReplaceDate(string content)
    {
        return Regex.Replace(content, match =>
        {
            var value = match.Value[7..^2];
            var dateNow = DateTime.Now;

            if (value.StartsWith('+') || value.StartsWith('-'))
            {
                if (int.TryParse(value[..^1].ToLower(), out var offset))
                {
                    var date = value[^1] switch
                    {
                        'y' => dateNow.AddYears(offset),
                        'm' => dateNow.AddMonths(offset),
                        'w' => dateNow.AddDays(offset * 7),
                        'd' => dateNow.AddDays(offset),
                        _ => dateNow
                    };

                    return date.ToString("dd MM yyyy");
                }
            }

            var parsed = value.ToLower() switch
            {
                "childbirthyear" => dateNow.AddYears(-15),
                "adultbirthyear" => dateNow.AddYears(-40),
                "oapbirthyear" => dateNow.AddYears(-66),
                "nextweek" => dateNow.AddDays(7),
                "nextmonth" => dateNow.AddMonths(1),
                "nextyear" => dateNow.AddYears(1),
                "lastweek" => dateNow.AddDays(-7),
                "lastmonth" => dateNow.AddMonths(-1),
                "lastyear" => dateNow.AddYears(-1),
                _ => dateNow
            };

            return parsed.ToString("dd MM yyyy");
        });
    }

    public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
    {
        if (!string.IsNullOrEmpty(page.LeadingParagraph))
            page.LeadingParagraph = ReplaceDate(page.LeadingParagraph);

        page.Elements = page.Elements.Select(element =>
        {
            if (!string.IsNullOrEmpty(element.Properties?.Text))
                element.Properties.Text = ReplaceDate(element.Properties.Text);

            if (!string.IsNullOrEmpty(element.Properties?.Hint))
                element.Properties.Hint = ReplaceDate(element.Properties.Hint);

            if (!string.IsNullOrEmpty(element.Properties?.Label))
                element.Properties.Label = ReplaceDate(element.Properties.Label);

            return element;
        }).ToList();

        return page;
    }

    public string ParseString(string content, FormAnswers formAnswers) =>
        ReplaceDate(content);
}