using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks
{
    public class ConditionalElementsAreValidCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            List<IElement> elementsWithConditionalElements = schema.Pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.ValidatableElements)
                .Where(_ => _.Type.Equals(EElementType.Radio) || _.Type.Equals(EElementType.Checkbox))
                .Where(_ => _.Properties.Options.Any(_ => _.HasConditionalElement))
                .ToList();

            List<IElement> conditionalElements = schema.Pages.Where(page => page.Elements != null)
                .SelectMany(page => page.Elements)
                .Where(element => element.Properties.isConditionalElement)
                .ToList();

            foreach (var radio in elementsWithConditionalElements)
            {
                foreach (var option in radio.Properties.Options)
                {
                    if (option.HasConditionalElement 
                            && !string.IsNullOrEmpty(option.ConditionalElementId) 
                            && !conditionalElements.Any(_ => _.Properties.QuestionId == option.ConditionalElementId))
                        integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' does not contain a conditional element for the '{option.Value}' value of radio '{radio.Properties.QuestionId}'");

                    if (option.HasConditionalElement 
                            && !string.IsNullOrEmpty(option.ConditionalElementId)  
                            && !schema.Pages.Any(page => page.ValidatableElements.Contains(radio) 
                                        && page.Elements.Any(element => element.Properties.QuestionId == option.ConditionalElementId 
                                            && element.Properties.isConditionalElement)))
                        integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' contains the conditional element for the '{option.Value}' value of radio '{radio.Properties.QuestionId}' on a different page to the radio element");
                    
                    conditionalElements.Remove(conditionalElements.FirstOrDefault(_ => _.Properties.QuestionId == option.ConditionalElementId));
                }
            }

            if (conditionalElements.Count > 0)
                integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' has conditional elements '{ String.Join(", ", conditionalElements.Select(_ => _.Properties.QuestionId)) }' not assigned to radio options");

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}