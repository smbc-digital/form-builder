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
        public FormSchemaIntegrityValidator(IEnumerable<IFormSchemaIntegrityCheck> formSchemaIntegrityChecks)
        {
            _formSchemaIntegrityChecks = formSchemaIntegrityChecks;
        }
        
        public async Task Validate(FormSchema schema)
        {
            var integrityCheckResults = new List<IntegrityCheckResult>();
            foreach(var integrityCheck in _formSchemaIntegrityChecks)
            {
                integrityCheckResults.Add(await integrityCheck.ValidateAsync(schema));
            }
            
            var invalidCheckResults = integrityCheckResults.Where(result => !result.IsValid);
            if(invalidCheckResults.Any())
            {
                var failingCheckResultMessages = invalidCheckResults.SelectMany(result => result.Messages);
                throw new ApplicationException($"The requested for schema '{schema.FormName}' was invalid, The following integrity check results are failing: \n { String.Join('\n', failingCheckResultMessages) }");
            }
        }
    }
}