using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class HasDuplicateQuestionIdsCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();
            List<string> questionIds = new();

            foreach (var page in schema.Pages)
            {
                foreach (var element in page.ValidatableElements)
                {
                    questionIds.Add(element.Properties.QuestionId);

                    if (element.Properties.Elements is not null && element.Properties.Elements.Count > 0)
                    {
                        foreach (var nestedElement in element.Properties.Elements)
                        {
                            questionIds.Add(nestedElement.Properties.QuestionId);
                        }
                    }
                }
            }

            HashSet<string> hashSet = new();
            if (questionIds.Any(id => !hashSet.Add(id)))
            {
                result.AddFailureMessage(
                    "The provided json contains questions with the same QuestionID, " +
                    "All Questions must have unique QuestionIds, " +
                    $"QuestionId: {hashSet.First()}");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}