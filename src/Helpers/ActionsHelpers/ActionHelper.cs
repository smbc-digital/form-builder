using System.Linq;
using System.Text.RegularExpressions;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Services.RetrieveExternalDataService.Entities;
using form_builder.Services.ValidateService.Entities;

namespace form_builder.Helpers.ActionsHelpers
{
    public interface IActionHelper
    {
        ExternalDataEntity GenerateUrl(string baseUrl, FormAnswers formAnswers);

        string GetEmailToAddresses(IAction action, FormAnswers formAnswers);

        ValidateEntity GenerateDoumentUploadUrl(string baseUrl, FormAnswers formAnswers);
    }

    public class ActionHelper : IActionHelper
    {
        private static Regex TagRegex => new Regex("(?<={{).*?(?=}})", RegexOptions.Compiled);

        public ExternalDataEntity GenerateUrl(string baseUrl, FormAnswers formAnswers)
        {
            var matches = TagRegex.Matches(baseUrl);
            var newUrl = matches.Aggregate(baseUrl, (current, match) => Replace(match, current, formAnswers));
            return new ExternalDataEntity
            {
                Url = newUrl,
                IsPost = !matches.Any()
            };
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

            return emailList.Where(_ => _ != null).Aggregate("", (current, email) => current + email + ",");
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

        public ValidateEntity GenerateDoumentUploadUrl(string baseUrl, FormAnswers formAnswers)
        {
            var matches = TagRegex.Matches(baseUrl);
            var newUrl = matches.Aggregate(baseUrl, (current, match) => Replace(match, current, formAnswers));
            return new ValidateEntity
            {
                Url = newUrl,
                IsPost = false
            };
        }
    }
}