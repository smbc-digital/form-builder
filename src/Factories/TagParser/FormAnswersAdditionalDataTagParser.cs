using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.Factories.TagParser
{
    public class FormAnswersAdditionalDataTagParser : ITagParser
    {
        public Regex Regex => new Regex("(?<={{)ANSWERDATA:.*?(?=}})", RegexOptions.Compiled);

        public Page Parse(Page page, FormAnswers formAnswers)
        {
            var answersDictionary = formAnswers.AdditionalFormAnswersData;

            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties.Text))
                    element.Properties.Text = Parse(element.Properties.Text, answersDictionary);
                    
                return element;
            }).ToList();

            return page;
        }

        private string Parse(string value, Dictionary<string, object> answersDictionary)
        {
            var match = Regex.Match(value);
            if (match.Success)
            {
                var splitMatch = match.Value.Split(":");
                var questionId = splitMatch[1];

                if (string.IsNullOrEmpty(questionId))
                    throw new Exception($"FormAnswerTagParser::Parse, replacement values questionId is null or empty, Match value: {match.Value}");

                var questionValue = (string)answersDictionary[questionId];

                if (string.IsNullOrEmpty(questionValue))
                    throw new Exception($"FormAnswerTagParser::Parse, replacement value for quetionId {questionId} is null or empty, Match value: {match.Value}");

                var replacementText = new StringBuilder(value);
                replacementText.Remove(match.Index - 2, match.Length + 4);
                replacementText.Insert(match.Index - 2, questionValue);
                Parse(replacementText.ToString(), answersDictionary);
            }

            return value;
        }
    }
}