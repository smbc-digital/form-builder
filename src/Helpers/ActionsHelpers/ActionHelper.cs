using System.Text.RegularExpressions;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Services.RetrieveExternalDataService.Entities;
using form_builder.TagParsers.Formatters;

namespace form_builder.Helpers.ActionsHelpers
{
    public class ActionHelper : IActionHelper
    {
        private readonly IEnumerable<IFormatter> _formatters;
        private readonly ILogger<ActionHelper> _logger;
        private static Regex TagRegex => new Regex("(?<={{).*?(?=}})", RegexOptions.Compiled);
        public ActionHelper(IEnumerable<IFormatter> formatters, ILogger<ActionHelper> logger)
        {
            _formatters = formatters;
            _logger = logger;
        }

        public RequestEntity GenerateUrl(string baseUrl, FormAnswers formAnswers)
        {
            var matches = TagRegex.Matches(baseUrl);
            var newUrl = matches.Aggregate(baseUrl, (current, match) => Replace(match, current, formAnswers));

            return new RequestEntity
            {
                Url = newUrl,
                IsPost = !matches.Any()
            };
        }

        public string GetEmailContent(IAction action, FormAnswers formAnswers)
        {
            // Any {{variables}} to find
            string content = action.Properties.Content;
            if (!TagRegex.Matches(content).Any())
                return content;

            foreach (Match m in TagRegex.Matches(content))
            {
                if (m.Success)
                {
                    var splitMatch = m.Value.Split(":");
                    var questionKey = splitMatch[0];
                    var question = formAnswers.Pages
                            .SelectMany(_ => _.Answers)
                            .FirstOrDefault(_ => _.QuestionId.Equals(questionKey));

                    if (question is not null)
                    {
                        var answer = question.Response as string;

                        // Format on ":" split of string
                        if (splitMatch.Length > 1)
                            answer = _formatters.Get(splitMatch[1]).Parse(answer);

                        // Replace and return string...
                        content = content.Replace($"{{{{{m.Groups[0].Value}}}}}", answer);
                    }
                    else
                    {
                        // Found no such key : At least remove the variable name
                        content = content.Replace($"{{{{{m.Groups[0].Value}}}}}", string.Empty);
                    }
                }
            }

            return content;
        }

        public string GetEmailToAddresses(IAction action, FormAnswers formAnswers)
        {
            var matches = TagRegex.Matches(action.Properties.To).ToList();

            var emailList = matches
                .Select(match => RecursiveGetAnswerValue(match.Value, formAnswers.Pages
                    .SelectMany(_ => _.Answers)
                    .FirstOrDefault(_ => _.QuestionId.Equals(match.Value))))
                .ToList();

            emailList.AddRange(action.Properties.To.Split(",").Where(_ => !TagRegex.IsMatch(_)));

            emailList = emailList.Where(_ => _ is not null).ToList();

            if (emailList.Count > 0)
                return emailList.Aggregate((current, email) => current + "," + email);

            _logger.LogInformation($"{formAnswers.FormName}: form action email couldn't find and email in the submitted anwsers");

            return string.Empty;

        }

        private string Replace(Match match, string current, FormAnswers formAnswers)
        {
            var splitTargets = match.Value.Split(".");
            var formAnswerDictionary = formAnswers.Pages?.SelectMany(_ => _.Answers).Select(x => new Answers { QuestionId = x.QuestionId, Response = x.Response }).ToList();
            formAnswerDictionary.AddRange(formAnswers.AdditionalFormData.Select(x => new Answers { QuestionId = x.Key, Response = x.Value }).ToList());

            var answer = formAnswerDictionary.Any(a => a.QuestionId.Equals(splitTargets[0]))
                ? RecursiveGetAnswerValue(match.Value, formAnswerDictionary.First(a => a.QuestionId.Equals(splitTargets[0])))
                : string.Empty;

            return current.Replace($"{{{{{match.Groups[0].Value}}}}}", answer);
        }

        private string RecursiveGetAnswerValue(string targetMapping, Answers answer)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length.Equals(1))
                return (dynamic)answer?.Response;

            var subObject = new Answers { Response = (dynamic)answer.Response[splitTargets[1]] };
            return RecursiveGetAnswerValue(targetMapping.Replace($"{splitTargets[0]}.", string.Empty), subObject);
        }
    }
}