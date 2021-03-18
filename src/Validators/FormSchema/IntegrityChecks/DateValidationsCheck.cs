using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks
{
    public class DateValidationsCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            IEnumerable<IElement> elements = schema.Pages.SelectMany(element => element.ValidatableElements);

            elements.Where(element => !string.IsNullOrEmpty(element.Properties.IsDateAfter))
                .ToList()
                .ForEach(element => { 
                    if(!elements.Any(comparisonElement => comparisonElement.Properties.QuestionId == element.Properties.IsDateAfter)) 
                        integrityCheckResult.AddFailureMessage($"Date Validations Check, IsDateAfter validation, for question '{element.Properties.QuestionId}' - the form does not contain a comparison element with QuestionId '{element.Properties.IsDateAfter}'"); 
                });

            elements.Where(element => !string.IsNullOrEmpty(element.Properties.IsDateBefore))
                .ToList()
                .ForEach(element => { 
                    if(!elements.Any(comparisonElement => comparisonElement.Properties.QuestionId == element.Properties.IsDateBefore)) 
                        integrityCheckResult.AddFailureMessage($"Date Validations Check, IsDateBefore validation, for question '{element.Properties.QuestionId}' - the form does not contain a comparison element with question id '{element.Properties.IsDateBefore}'"); 
                });

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}