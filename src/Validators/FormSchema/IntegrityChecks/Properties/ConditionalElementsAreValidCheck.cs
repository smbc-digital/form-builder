using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Properties
{
    public class ConditionalElementsAreValidCheck: IPropertySchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            var radioWithConditionals = schema.Pages.Where(page => page.Elements != null)
                .SelectMany(page => page.ValidatableElements)
                .Where(element => element.Type == EElementType.Radio)
                .Where(element => element.Properties.Options.Any(_ => _.HasConditionalElement))
                .ToList();

            var conditionalElements = schema.Pages.Where(page => page.Elements != null)
                .SelectMany(page => page.Elements)
                .Where(element => element.Properties.isConditionalElement)
                .ToList();

            foreach (var radio in radioWithConditionals)
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