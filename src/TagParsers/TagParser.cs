using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace form_builder.TagParser
{
    public class TagParser
    {
        public string Parse(string value, Dictionary<string, object> answersDictionary, Regex regex)
        {
            var match = regex.Match(value);
            if (match.Success)
            {
                var splitMatch = match.Value.Split(":");
                var questionId = splitMatch[1];

                if(!answersDictionary.ContainsKey(questionId))
                    throw new ApplicationException($"FormAnswerTagParser::Parse, replacement value for quetionId {questionId} is not stored within answers, Match value: {match.Value}");

                var questionValue = (string)answersDictionary[questionId];

                if (string.IsNullOrEmpty(questionValue))
                    throw new ApplicationException($"FormAnswerTagParser::Parse, replacement value for quetionId {questionId} is null or empty, Match value: {match.Value}");

                var replacementText = new StringBuilder(value);
                replacementText.Remove(match.Index - 2, match.Length + 4);
                replacementText.Insert(match.Index - 2, questionValue);
                return Parse(replacementText.ToString(), answersDictionary, regex);
            }

            return value;
        }
    }
}