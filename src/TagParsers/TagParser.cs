using System.Text;
using System.Text.RegularExpressions;
using form_builder.Extensions;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers;

public class TagParser
{
    private readonly IEnumerable<IFormatter> _formatters;
    public TagParser(IEnumerable<IFormatter> formatters)
    {
        _formatters = formatters;
    }

    public string Parse(string value, Dictionary<string, object> answersDictionary, Regex regex, bool allowOptional = false)
    {
        Match match = regex.Match(value);

        if (match.Success)
        {
            StringBuilder replacementText = new(value);
            string[] splitMatch = match.Value.Split(":");
            string questionId = splitMatch[1];

            string format = string.Empty;
            if (splitMatch.Length > 2)
                format = splitMatch[2];

            if (!answersDictionary.ContainsKey(questionId) || string.IsNullOrEmpty((string)answersDictionary[questionId]))
            {
                if (allowOptional)
                {
                    replacementText.Remove(match.Index - 2, match.Length + 4);
                    replacementText.Insert(match.Index - 2, string.Empty);
                    return Parse(replacementText.ToString(), answersDictionary, regex, allowOptional);
                }

                if (!answersDictionary.ContainsKey(questionId))
                    throw new ApplicationException($"FormAnswerTagParser::Parse, replacement value for questionId {questionId} is not stored within answers, Match value: {match.Value}");

                throw new ApplicationException($"FormAnswerTagParser::Parse, replacement value for questionId {questionId} is null or empty, Match value: {match.Value}");
            }

            string questionValue = (string)answersDictionary[questionId];

            if (!string.IsNullOrEmpty(format))
                questionValue = _formatters.Get(format).Parse(questionValue);

            replacementText.Remove(match.Index - 2, match.Length + 4);
            replacementText.Insert(match.Index - 2, questionValue);
            return Parse(replacementText.ToString(), answersDictionary, regex, allowOptional);
        }

        return value;
    }


    public string Parse(string value, Regex regex, string data, Func<string[], string> formatContent, string split = ":")
    {
        Match match = regex.Match(value);
        if (match.Success)
        {
            string[] splitMatch = match.Value.Split(split);
            string[] content = splitMatch.Skip(1).Take(splitMatch.Length - 1).ToArray();

            StringBuilder replacementText = new(value);
            replacementText.Remove(match.Index - 2, match.Length + 4);
            replacementText.Insert(match.Index - 2, formatContent(content));
            return Parse(replacementText.ToString(), regex, data, formatContent);
        }

        return value;
    }

    public string Parse(string value, string parseValue, Regex regex)
    {
        Match match = regex.Match(value);
        if (match.Success)
        {
            StringBuilder replacementText = new(value);
            replacementText.Remove(match.Index - 2, match.Length + 4);
            replacementText.Insert(match.Index - 2, parseValue);
            return replacementText.ToString();
        }

        return value;
    }

    public string Parse(string value, Regex regex)
    {
        Match match = regex.Match(value);
        if (match.Success)
        {
            string[] parser = match.Value.Split("::");
            string parserType = parser[0];
            string parserValue = string.Empty;
            if (parser.Length > 1)
                parserValue = parser[1];

            StringBuilder replacementText = new(value);
            replacementText.Remove(match.Index - 2, match.Length + 4);
            switch (parserType)
            {
                case "STRONG":
                    replacementText.Insert(match.Index - 2, $"<strong>{parserValue}</strong>");
                    break;
                case "BOLD":
                    replacementText.Insert(match.Index - 2, $"<b>{parserValue}</b>");
                    break;
                case "EMPHASIS":
                    replacementText.Insert(match.Index - 2, $"<em>{parserValue}</em>");
                    break;
                case "ITALIC":
                    replacementText.Insert(match.Index - 2, $"<i>{parserValue}</i>");
                    break;
                case "BREAK":
                    replacementText.Insert(match.Index - 2, "<br>");
                    break;
            }

            return Parse(replacementText.ToString(), regex);
        }

        return value;
    }

    public string ParseList(string value, Regex regex, bool noClasses)
    {
        Match match = regex.Match(value);
        if (match.Success)
        {
            string[] parser = match.Value.Split("::");
            string parserType = parser[0];
            string[] parserValue = parser[1].Split("|");
            StringBuilder replacementText = new(value);
            replacementText.Remove(match.Index - 2, match.Length + 4);
            StringBuilder listHtml = new();

            if (noClasses)
                listHtml.Append(parserType.Equals("ULIST") ? "<ul>" : "<ol>");
            else
                listHtml.Append(parserType.Equals("ULIST") ? "<ul class='govuk-list govuk-list--bullet'>" : "<ol class='govuk-list govuk-list--number'>");

            foreach (string item in parserValue)
            {
                listHtml.Append($"<li>{item}</li>");
            }

            listHtml.Append(parserType.Equals("ULIST") ? "</ul>" : "</ol>");
            replacementText.Insert(match.Index - 2, listHtml);

            return ParseList(replacementText.ToString(), regex, noClasses);
        }

        return value;
    }

    public string ParseDate(string value, Regex regex)
    {
        return regex.Replace(value, match =>
        {
            string parserValue = match.Value[8..^2];
            DateTime dateNow = DateTime.Now;

            if (parserValue.StartsWith('+') || parserValue.StartsWith('-'))
            {
                if (int.TryParse(parserValue[..^1], out int offset))
                {
                    DateTime date = parserValue[^1] switch
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

            DateTime parsed = parserValue.ToLower() switch
            {
                "childbirthyear" => dateNow.AddYears(-15),
                "adultbirthyear" => dateNow.AddYears(-40),
                "oapbirthyear" => dateNow.AddYears(-70),
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
}
