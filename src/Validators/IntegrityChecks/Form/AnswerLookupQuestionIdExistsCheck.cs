using form_builder.Models;
using form_builder.Models.Actions;
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

                IAction matchingPageAction = schema.Pages
                    .SelectMany(page => page.PageActions)
                    .SingleOrDefault(action =>
                        action.Properties.TargetQuestionId is not null &&
                        action.Properties.TargetQuestionId.Equals(questionId));

                if (matchingPageAction is null)
                    result.AddFailureMessage($"The provided json does not contain a retrieve external data page action with questionId of '{questionId}'");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
