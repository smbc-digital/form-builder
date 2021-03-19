using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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

            List<IElement> radioWithConditionals = schema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.ValidatableElements)
                .Where(element => element.Type.Equals(EElementType.Radio))
                .Where(element => element.Properties.Options.Any(option => option.HasConditionalElement))
                .ToList();

            List<IElement> conditionalElements = schema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.Elements)
                .Where(element => element.Properties.isConditionalElement)
                .ToList();

            foreach (var radio in radioWithConditionals)
            {
                foreach (var option in radio.Properties.Options)
                {
                    if (option.HasConditionalElement &&
                        !string.IsNullOrEmpty(option.ConditionalElementId) &&
                        !conditionalElements.Any(element => element.Properties.QuestionId.Equals(option.ConditionalElementId)))
                            result.AddFailureMessage($"The provided json '{schema.FormName}' does not contain a conditional element for the '{option.Value}' value of radio '{radio.Properties.QuestionId}'");

                    if (option.HasConditionalElement && !string.IsNullOrEmpty(option.ConditionalElementId) &&
                        !schema.Pages.Any(page => page.ValidatableElements.Contains(radio) &&
                        page.Elements.Any(element => element.Properties.QuestionId is not null && element.Properties.QuestionId.Equals(option.ConditionalElementId) &&
                        element.Properties.isConditionalElement)))
                            result.AddFailureMessage($"The provided json '{schema.FormName}' contains the conditional element for the '{option.Value}' value of radio '{radio.Properties.QuestionId}' on a different page to the radio element");

                    conditionalElements.Remove(conditionalElements
                        .FirstOrDefault(element => element.Properties.QuestionId.Equals(option.ConditionalElementId)));
                }
            }

            if (conditionalElements.Count > 0)
                result.AddFailureMessage($"The provided json has conditional elements '{ String.Join(", ", conditionalElements.Select(_ => _.Properties.QuestionId)) }' not assigned to radio options");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
