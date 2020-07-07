﻿using System.Linq;
using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.Services.RetrieveExternalDataService.Entities;

namespace form_builder.Helpers.ActionsHelpers
{
    public interface IPageActionsHelper
    {
        ExternalDataEntity GenerateUrl(string baseUrl, FormAnswers formAnswers);
    }

    public class PageActionsHelper : IPageActionsHelper
    {
        private Regex _tagRegex => new Regex("(?<={{).*?(?=}})", RegexOptions.Compiled);

        public ExternalDataEntity GenerateUrl(string baseUrl, FormAnswers formAnswers)
        {
            var matches = _tagRegex.Matches(baseUrl);
            var newUrl = matches.Aggregate(baseUrl, (current, match) => Replace(match, current, formAnswers));
            return new ExternalDataEntity
            {
                Url = newUrl,
                IsPost = !matches.Any()
            };
        }

        private string Replace(Match match, string current, FormAnswers formAnswers)
        {
            var splitTargets = match.Value.Split(".");
            var answer = RecursiveGetAnswerValue(match.Value, formAnswers.Pages.SelectMany(_ => _.Answers).First(a => a.QuestionId.Equals(splitTargets[0])));

            return current.Replace($"{{{{{match.Groups[0].Value}}}}}", answer);
        }

        private string RecursiveGetAnswerValue(string targetMapping, Answers answer)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length == 1)
                return (dynamic)answer.Response;

            var subObject = new Answers { Response = (dynamic)answer.Response[splitTargets[1]] };
            return RecursiveGetAnswerValue(targetMapping.Replace($"{splitTargets[0]}.", string.Empty), subObject);
        }
    }
}