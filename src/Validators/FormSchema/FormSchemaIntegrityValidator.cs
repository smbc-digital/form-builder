using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;

namespace form_builder.Validators
{
    public class FormSchemaIntegrityValidator : IFormSchemaIntegrityValidator
    {
        IEnumerable<IFormSchemaIntegrityCheck> _formSchemaIntegrityChecks;

        public FormSchemaIntegrityValidator(IEnumerable<IFormSchemaIntegrityCheck> formSchemaIntegrityChecks) =>
            _formSchemaIntegrityChecks = formSchemaIntegrityChecks;
        
        public async Task Validate(FormSchema schema)
        {
            var integrityCheckResults = new List<IntegrityCheckResult>();

            _formSchemaIntegrityChecks.ToList().ForEach(async check => integrityCheckResults.Add(await check.ValidateAsync(schema)));
            
            IEnumerable<IntegrityCheckResult> invalidCheckResults = integrityCheckResults.Where(result => !result.IsValid);

            if (invalidCheckResults.Any())
            {
                var failingCheckResultMessages = invalidCheckResults.SelectMany(result => result.Messages);
                throw new ApplicationException($"The requested for schema '{schema.FormName}' was invalid, The following integrity check results are failing: \n { String.Join('\n', failingCheckResultMessages) }");
            }
        }
    }
}