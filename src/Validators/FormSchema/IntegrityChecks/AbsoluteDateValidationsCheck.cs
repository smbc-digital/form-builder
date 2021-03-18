using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks
{
    public class AbsoluteDateValidationsCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            IEnumerable<IElement> elements = schema.Pages.SelectMany(element => element.ValidatableElements);
            
            elements.Where(element => !string.IsNullOrEmpty(element.Properties.IsDateAfterAbsolute))
                .ToList()
                .ForEach(element => { 
                    if (!DateTime.TryParse(element.Properties.IsDateAfterAbsolute, out DateTime outputDate))
                        integrityCheckResult.AddFailureMessage($"Absolute Date Validations Check, IsDateAfterAbsolute validation, '{element.Properties.QuestionId}' does not provide a valid comparison date in form '{schema.FormName}'"); 
                });

            elements.Where(element => !string.IsNullOrEmpty(element.Properties.IsDateBeforeAbsolute))
                .ToList()
                .ForEach(element => { 
                    if (!DateTime.TryParse(element.Properties.IsDateBeforeAbsolute, out DateTime outputDate))
                        integrityCheckResult.AddFailureMessage($"Absolute Date Validations Check, IsDateBeforeAbsolute validation, '{element.Properties.QuestionId}' does not provide a valid comparison date in form '{schema.FormName}'");
                });   
            
            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}