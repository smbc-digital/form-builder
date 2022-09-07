using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class AnswerLookupQuestionIdExistsCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();
            IEnumerable<IElement> elements = schema.Pages
                .SelectMany(page => page.Elements)
                .Where(element => element.Lookup is not null &&
                       element.Lookup.StartsWith("#"));

            if (!elements.Any()) return result;

            foreach (var element in elements)
            {
                string questionId = element.Lookup.TrimStart('#');
                IElement matchingElement = schema.Pages
                    .SelectMany(page => page.Elements)
                    .SingleOrDefault(element =>
                        element.Properties.QuestionId is not null &&
                        element.Properties.QuestionId.Equals(questionId));

                if (matchingElement is null)
                    result.AddFailureMessage($"The provided json does not contain an element with questionId of '{questionId}'");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
