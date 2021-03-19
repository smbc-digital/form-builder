using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks
{
    public class IncomingFormDataValuesCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (schema.Pages.Any(page => page.HasIncomingValues))
            {
                schema.Pages.Where(page => page.HasIncomingValues)
                    .ToList()
                    .ForEach(page => page.IncomingValues.ForEach(incomingValue =>
                        {
                            if (incomingValue.HttpActionType.Equals(EHttpActionType.Unknown))
                                integrityCheckResult.AddFailureMessage($"Incoming Form DataValues Check, EHttpActionType cannot be unknown, set to Get or Post for incoming value '{incomingValue.Name}' on page '{page.Title}' in form '{schema.FormName}'");

                            if (string.IsNullOrEmpty(incomingValue.QuestionId) || string.IsNullOrEmpty(incomingValue.Name))
                                integrityCheckResult.AddFailureMessage($"Incoming Form DataValues Check, QuestionId or Name cannot be empty on page '{page.Title}' in form '{schema.FormName}'");
                        }
                    ));
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}