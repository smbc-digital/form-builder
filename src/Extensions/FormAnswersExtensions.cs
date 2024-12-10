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
                answer.Pages?.SelectMany(_ => _.Answers).ToDictionary(x => x.QuestionId, x => x.Response),
                schema.Pages,
                !String.IsNullOrEmpty(answer.Path) && answer.Path.Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH) ? answer.Path : schema.FirstPageSlug,
                new List<PageAnswers>());

        private static List<PageAnswers> RecursivelyReduceAnswers(
            List<PageAnswers> answers,
            Dictionary<string, object> answersDictionary,
            List<Page> schema,
            string currentPageSlug,
            List<PageAnswers> reducedAnswers)
        {
            var page = new Page();
            var currentAnswer = answers?.Find(_ => _.PageSlug.Equals(currentPageSlug));
            if (currentAnswer is null)
                return reducedAnswers;

            var currentSchema = schema.FindAll(_ => _.PageSlug.Equals(currentPageSlug));
            if (currentSchema is null)
                return reducedAnswers;

            page = currentSchema.Count > 1
                ? currentSchema.FirstOrDefault(page => page.CheckPageMeetsConditions(answersDictionary))
                : currentSchema.FirstOrDefault();

            if (page is null)
                return reducedAnswers;

            reducedAnswers.Add(currentAnswer);

            var behaviour = page.GetNextPage(answersDictionary);
            if (!behaviour.BehaviourType.Equals(EBehaviourType.GoToPage))
                return reducedAnswers;

            return RecursivelyReduceAnswers(answers, answersDictionary, schema, behaviour.PageSlug, reducedAnswers);
        }
    }
}