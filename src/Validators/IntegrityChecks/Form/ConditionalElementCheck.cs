using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class ConditionalElementCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            List<IElement> elementsWithConditionals = schema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.ValidatableElements)
                .Where(element => element.Type.Equals(EElementType.Radio) || element.Type.Equals(EElementType.Checkbox))
                .Where(element => element.Properties.Options.Any(option => option.HasConditionalElement))
                .ToList();

            List<IElement> conditionalElements = schema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.Elements)
                .Where(element => element.Properties.isConditionalElement)
                .ToList();

            if (elementsWithConditionals.Count.Equals(0))
                return result;

            foreach (var element in elementsWithConditionals)
            {
                foreach (var option in element.Properties.Options)
                {
                    if (option.HasConditionalElement &&
                        !string.IsNullOrEmpty(option.ConditionalElementId) &&
                        !conditionalElements.Any(element => element.Properties.QuestionId.Equals(option.ConditionalElementId)))
                    {
                        result.AddFailureMessage(
                            $"The provided json does not contain a conditional element for the '{option.Value}' value of element '{element.Properties.QuestionId}'");
                    }

                    if (option.HasConditionalElement && !string.IsNullOrEmpty(option.ConditionalElementId) &&
                        !schema.Pages.Any(page => page.ValidatableElements.Contains(element) &&
                        page.Elements.Any(element => element.Properties.QuestionId is not null && element.Properties.QuestionId.Equals(option.ConditionalElementId) &&
                        element.Properties.isConditionalElement)))
                    {
                        result.AddFailureMessage(
                            $"The provided json contains the conditional element for the '{option.Value}' value of element '{element.Properties.QuestionId}' on a different page to the conditional element");
                    }

                    conditionalElements.Remove(conditionalElements
                        .FirstOrDefault(element => element.Properties.QuestionId.Equals(option.ConditionalElementId)));
                }
            }

            if (conditionalElements.Count > 0)
                result.AddFailureMessage($"The provided json has conditional elements '{String.Join(", ", conditionalElements.Select(element => element.Properties.QuestionId))}' not assigned to element options");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
