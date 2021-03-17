using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder.Validators.IntegrityChecks.Behaviours;
using form_builder.Validators.IntegrityChecks.Elements;
using form_builder.Validators.IntegrityChecks.FormSchema;
using form_builder.Validators.IntegrityChecks.Page;
using form_builder.Validators.IntegrityChecks.Properties;

namespace form_builder.Validators
{
    public class FormSchemaIntegrityValidator : IFormSchemaIntegrityValidator
    {
        IEnumerable<IFormSchemaIntegrityCheck> _formSchemaIntegrityChecks;
        IEnumerable<IPageSchemaIntegrityCheck> _pageSchemaIntegrityChecks;
        IEnumerable<IBehaviourSchemaIntegrityCheck> _behaviorSchemaIntegrityChecks;
        IEnumerable<IElementSchemaIntegrityCheck> _elementSchemaIntegrityChecks;
        IEnumerable<IPropertySchemaIntegrityCheck> _propertySchemaIntegrityChecks;

        public FormSchemaIntegrityValidator(
            IEnumerable<IFormSchemaIntegrityCheck> formSchemaIntegrityChecks,
            IEnumerable<IPageSchemaIntegrityCheck> pageSchemaIntegrityChecks,
            IEnumerable<IBehaviourSchemaIntegrityCheck> behaviorSchemaIntegrityChecks,
            IEnumerable<IElementSchemaIntegrityCheck> elementSchemaIntegrityChecks,
            IEnumerable<IPropertySchemaIntegrityCheck> propertySchemaIntegrityChecks)
        {
            _formSchemaIntegrityChecks = formSchemaIntegrityChecks;
            _pageSchemaIntegrityChecks = pageSchemaIntegrityChecks;
            _behaviorSchemaIntegrityChecks = behaviorSchemaIntegrityChecks;
            _elementSchemaIntegrityChecks = elementSchemaIntegrityChecks;
            _propertySchemaIntegrityChecks = propertySchemaIntegrityChecks;
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
                // Page : 
                foreach (var integrityCheck in _pageSchemaIntegrityChecks)
                {
                    integrityCheckResults.Add(await integrityCheck.ValidateAsync(page));
                }


                if (page.Behaviours != null)
                {
                    // Page Behaviours : 
                    foreach (var behaviour in page.Behaviours)
                    {
                        if (behaviour.BehaviourType.Equals(EBehaviourType.SubmitForm) ||
                            behaviour.BehaviourType.Equals(EBehaviourType.SubmitAndPay))
                        {
                            foreach (var integrityCheck in _behaviorSchemaIntegrityChecks)
                            {
                                integrityCheckResults.Add(await integrityCheck.ValidateAsync(behaviour));
                            }
                        }
                        else
                        {

                        }
                    }

                    // Page Elements : 
                    if (!page.Elements.Any())
                        continue;

                    foreach (var element in page.Elements)
                    {
                        // Page Element
                        foreach (var integrityCheck in _elementSchemaIntegrityChecks)
                        {
                            integrityCheckResults.Add(await integrityCheck.ValidateAsync(element, form));
                        }

                        // Page Element Properties

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
