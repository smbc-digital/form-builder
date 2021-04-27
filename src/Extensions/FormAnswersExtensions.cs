using System.Collections.Generic;
using System.Linq;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Extensions
{
    public static class FormAnswersExtensions
    {
        public static List<PageAnswers> GetReducedAnswers(this FormAnswers answer, FormSchema schema)
            => RecursivelyReduceAnswers(
                answer.Pages,
                answer.Pages.SelectMany(_ => _.Answers).ToDictionary(x => x.QuestionId, x => x.Response),
                schema.Pages,
                !string.IsNullOrEmpty(answer.Path) && answer.Path.Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH) ? answer.Path : schema.FirstPageSlug,
                new List<PageAnswers>());

        private static List<PageAnswers> RecursivelyReduceAnswers(
            List<PageAnswers> answers,
            Dictionary<string, object> answersDictionary,
            List<Page> schema,
            string currentPageSlug,
            List<PageAnswers> reducedAnswers)
        {
            var currentAnswer = answers.Find(_ => _.PageSlug.Equals(currentPageSlug));

            var currentSchema = schema.FindAll(_ => _.PageSlug.Equals(currentPageSlug));
            if (!currentSchema.Any())
                return reducedAnswers;

            var page = currentSchema.Count > 1
                ? currentSchema.FirstOrDefault(currentPage => currentPage.CheckPageMeetsConditions(answersDictionary))
                : currentSchema.FirstOrDefault();

            if (page is null)
                return reducedAnswers;

            if (currentAnswer is not null)
                reducedAnswers.Add(currentAnswer);

            var behaviour = page.GetNextPage(answersDictionary);
            if (behaviour is null || behaviour.BehaviourType != EBehaviourType.GoToPage)
                return reducedAnswers;

            return RecursivelyReduceAnswers(answers, answersDictionary, schema, behaviour.PageSlug, reducedAnswers);
        }
    }
}