using form_builder.Constants;
using form_builder.Models;

namespace form_builder.Extensions
{
    public static class FormSchemaExtensions
    {
        public static List<Page> GetReducedPages(this FormSchema schema, FormAnswers answers)
            => RecursivelyReducePages(
                answers.Pages?.SelectMany(_ => _.Answers).ToDictionary(x => x.QuestionId, x => x.Response),
                schema.Pages,
                !string.IsNullOrEmpty(answers.Path) && answers.Path.Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH) ? answers.Path : schema.FirstPageSlug,
                new List<Page>());

        private static List<Page> RecursivelyReducePages(
            Dictionary<string, object> answersDictionary,
            List<Page> schemaPages,
            string currentPageSlug,
            List<Page> reducedPages)
        {
            var page = new Page();
            var currentSchema = schemaPages.FindAll(_ => _.PageSlug.Equals(currentPageSlug));

            if (!currentSchema.Any())
                return reducedPages;

            page = currentSchema.Count > 1
                ? currentSchema.FirstOrDefault(page => page.CheckPageMeetsConditions(answersDictionary))
                : currentSchema.FirstOrDefault();

            if (page is null)
                return reducedPages;

            reducedPages.Add(page);

            if (currentPageSlug.Equals("success"))
                return reducedPages;

            var behaviour = page.GetNextPage(answersDictionary);

            if (behaviour is null || string.IsNullOrEmpty(behaviour.PageSlug))
                return reducedPages;

            return RecursivelyReducePages(answersDictionary, schemaPages, behaviour.PageSlug, reducedPages);
        }
    }
}
