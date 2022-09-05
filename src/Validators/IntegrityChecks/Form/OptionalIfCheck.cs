using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class OptionalIfCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();
            List<string> questionIds = new();

            IEnumerable<IElement> elements = schema.Pages
                .Where(page => page.ValidatableElements is not null)
                .SelectMany(page => page.ValidatableElements)
                .Where(element => !string.IsNullOrEmpty(element.Properties.OptionalIf.QuestionId) ||
                !string.IsNullOrEmpty(element.Properties.OptionalIf.ComparisonValue) ||
                !element.Properties.OptionalIf.ConditionType.Equals(ECondition.Undefined));

            if (!elements.Any())
                return result;

            foreach (var page in schema.Pages)
            {
                foreach (var element in page.ValidatableElements)
                {
                    questionIds.Add(element.Properties.QuestionId);

                    if (element.Properties.Elements is not null && element.Properties.Elements.Count > 0)
                    {
                        foreach (var nestedElement in element.Properties.Elements)
                            questionIds.Add(nestedElement.Properties.QuestionId);
                    }
                }
            }

            foreach (var element in elements)
            {
                if (string.IsNullOrEmpty(element.Properties.OptionalIf.QuestionId))
                    result.AddFailureMessage(
                           $"The provided json has an OptionalIf that does not contain a QuestionId. Error on: '{element.Properties.QuestionId}'");

                if (string.IsNullOrEmpty(element.Properties.OptionalIf.ComparisonValue))
                    result.AddFailureMessage(
                           $"The provided json has an OptionalIf that does not contain a ComparisonValue. Error on: '{element.Properties.QuestionId}'");

                if (element.Properties.OptionalIf.ConditionType.Equals(ECondition.Undefined))
                    result.AddFailureMessage(
                           $"The provided json contains an OptionalIf that does not contain a ConditionType. Error on: '{element.Properties.QuestionId}'");

                if (!string.IsNullOrEmpty(element.Properties.OptionalIf.QuestionId) && !questionIds.Contains(element.Properties.OptionalIf.QuestionId))
                    result.AddFailureMessage(
                           $"The provided json contains an OptionalIf that does not match to a QuestionId in the form. Error on: '{element.Properties.QuestionId}'");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}