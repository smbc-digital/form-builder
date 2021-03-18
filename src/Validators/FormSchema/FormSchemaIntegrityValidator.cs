using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder.Validators.IntegrityChecks.Behaviours;
using form_builder.Validators.IntegrityChecks.Elements;
using form_builder.Validators.IntegrityChecks.FormSchema;

namespace form_builder.Validators
{
    public class FormSchemaIntegrityValidator : IFormSchemaIntegrityValidator
    {
        IEnumerable<IFormSchemaIntegrityCheck> _formSchemaIntegrityChecks;
        IEnumerable<IBehaviourSchemaIntegrityCheck> _behaviorSchemaIntegrityChecks;
        IEnumerable<IElementSchemaIntegrityCheck> _elementSchemaIntegrityChecks;

        public FormSchemaIntegrityValidator(
            IEnumerable<IFormSchemaIntegrityCheck> formSchemaIntegrityChecks,
            IEnumerable<IBehaviourSchemaIntegrityCheck> behaviorSchemaIntegrityChecks,
            IEnumerable<IElementSchemaIntegrityCheck> elementSchemaIntegrityChecks)
        {
            _formSchemaIntegrityChecks = formSchemaIntegrityChecks;
            _behaviorSchemaIntegrityChecks = behaviorSchemaIntegrityChecks;
            _elementSchemaIntegrityChecks = elementSchemaIntegrityChecks;
        }

        public async Task Validate(FormSchema schema)
        {
            var integrityCheckResults = new List<IntegrityCheckResult>();

            // FormSchema Check
            foreach (var integrityCheck in _formSchemaIntegrityChecks)
            {
                integrityCheckResults.Add(await integrityCheck.ValidateAsync(schema));
            }

            foreach (var page in schema.Pages)
            {
                if (page.Behaviours is not null)
                {
                    foreach (var integrityCheck in _behaviorSchemaIntegrityChecks)
                    {
                        integrityCheckResults.Add(await integrityCheck.ValidateAsync(page.Behaviours));
                    }

                    // Page Elements : 
                    if (!page.Elements.Any())
                        continue;

                    foreach (var element in page.Elements)
                    {
                        // Page Element
                        foreach (var integrityCheck in _elementSchemaIntegrityChecks)
                        {
                            integrityCheckResults.Add(await integrityCheck.ValidateAsync(element));
                        }
                    }
                }
            }

            var invalidCheckResults = integrityCheckResults.Where(result => !result.IsValid);
            if (invalidCheckResults.Any())
            {
                var failingCheckResultMessages = invalidCheckResults.SelectMany(result => result.Messages);
                throw new ApplicationException($"The requested for schema '{schema.FormName}' was invalid, The following integrity check results are failing: \n { String.Join('\n', failingCheckResultMessages) }");
            }
        }
    }
}
