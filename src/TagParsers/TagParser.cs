using System.Text;
using System.Text.RegularExpressions;
using form_builder.Extensions;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class TagParser
    {
        private readonly IEnumerable<IFormatter> _formatters;
        public TagParser(IEnumerable<IFormatter> formatters) => _formatters = formatters;

        public string Parse(string value, Dictionary<string, object> answersDictionary, Regex regex)
        {
            var match = regex.Match(value);
            if (match.Success)
            {
                var splitMatch = match.Value.Split(":");
                var questionId = splitMatch[1];

                var format = string.Empty;
                if (splitMatch.Length > 2)
                    format = splitMatch[2];

                if (!answersDictionary.ContainsKey(questionId))
                    throw new ApplicationException($"FormAnswerTagParser::Parse, replacement value for questionId {questionId} is not stored within answers, Match value: {match.Value}");

                var questionValue = (string)answersDictionary[questionId];

                if (string.IsNullOrEmpty(questionValue))
                    throw new ApplicationException($"FormAnswerTagParser::Parse, replacement value for questionId {questionId} is null or empty, Match value: {match.Value}");

                if (!string.IsNullOrEmpty(format))
                    questionValue = _formatters.Get(format).Parse(questionValue);

                var replacementText = new StringBuilder(value);
                replacementText.Remove(match.Index - 2, match.Length + 4);
                replacementText.Insert(match.Index - 2, questionValue);
                return Parse(replacementText.ToString(), answersDictionary, regex);
            }

            return value;
        }


        public string Parse(string value, Regex regex, string data, Func<string[], string> formatContent)
        {
            var match = regex.Match(value);
            if (match.Success)
            {
                var splitMatch = match.Value.Split(":");
                var content = splitMatch.Skip(1).Take(splitMatch.Length - 1).ToArray();

                var replacementText = new StringBuilder(value);
                replacementText.Remove(match.Index - 2, match.Length + 4);
                replacementText.Insert(match.Index - 2, formatContent(content));
                return Parse(replacementText.ToString(), regex, data, formatContent);
            }

            return value;
        }

        public string Parse(string value, string parseValue, Regex regex)
        {
            var match = regex.Match(value);
            if (match.Success)
            {
                var replacementText = new StringBuilder(value);
                replacementText.Remove(match.Index - 2, match.Length + 4);
                replacementText.Insert(match.Index - 2, parseValue);
                return replacementText.ToString();
            }

            return value;
        }

        public string Parse(string value, Regex regex)
        {
            var match = regex.Match(value);
            if (match.Success)
            {
                var parser = match.Value.Split(":");
                var parserType = parser[0];
                var parserValue = parser[1];
                var replacementText = new StringBuilder(value);
                replacementText.Remove(match.Index - 2, match.Length + 4);
                switch (parserType)
                {
                    case "STRONG":
                        replacementText.Insert(match.Index - 2, $"<strong>{parserValue}</strong>");
                        break;
                    case "BOLD":
                        replacementText.Insert(match.Index - 2, $"<b>{parserValue}</b>");
                        break;
                }

                return Parse(replacementText.ToString(), regex);
            }

            return value;
        } 
    }
}