using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;

namespace form_builder.Validators
{
    public class FormSchemaIntegrityValidator : IFormSchemaIntegrityValidator
    {
        List<IFormSchemaIntegrityCheck> _formSchemaIntegrityChecks;
        public FormSchemaIntegrityValidator(List<IFormSchemaIntegrityCheck> formSchemaIntegrityChecks)
        {
            _formSchemaIntegrityChecks = formSchemaIntegrityChecks;
        }
        public async Task<bool> Validate(FormSchema schema, string form, string path)
        {
            if (path !=schema.FirstPageSlug)
                return true;

            var integrityCheckResults = new List<IntegrityCheckResult>();

            foreach(var integrityCheck in _formSchemaIntegrityChecks)
            {
                integrityCheckResults.Add(await integrityCheck.ValidateAsync(schema, form));
            }
            
            return integrityCheckResults.Any(integrityCheckResult => !integrityCheckResult.IsValid);
        }
    }
}