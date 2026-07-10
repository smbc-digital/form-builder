namespace form_builder.Validators.IntegrityChecks;

public class FormSchemaIntegrityValidator(IEnumerable<IFormSchemaIntegrityCheck> formSchemaIntegrityChecks,
    IEnumerable<IBehaviourSchemaIntegrityCheck> behaviorSchemaIntegrityChecks,
    IEnumerable<IElementSchemaIntegrityCheck> elementSchemaIntegrityChecks)
    : IFormSchemaIntegrityValidator
{
    public async Task Validate(FormSchema schema)
    {
        List<IntegrityCheckResult> integrityCheckResults = new();

        foreach (var integrityCheck in formSchemaIntegrityChecks)
        {
            integrityCheckResults.Add(await integrityCheck.ValidateAsync(schema));
        }

        foreach (var page in schema.Pages)
        {
            if (page.Behaviours is not null)
            {
                foreach (var integrityCheck in behaviorSchemaIntegrityChecks)
                {
                    integrityCheckResults.Add(await integrityCheck.ValidateAsync(page.Behaviours));
                }

                if (!page.Elements.Any())
                    continue;

                foreach (var element in page.Elements)
                {
                    if (element.Properties is not null &&
                        element.Properties.Elements is not null &&
                        element.Properties.Elements.Any())
                    {
                        foreach (var nestedElement in element.Properties.Elements)
                        {
                            foreach (var integrityCheck in elementSchemaIntegrityChecks)
                            {
                                integrityCheckResults.Add(await integrityCheck.ValidateAsync(nestedElement));
                            }
                        }
                    }

                    foreach (var integrityCheck in elementSchemaIntegrityChecks)
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
            throw new ApplicationException($"The requested for schema '{schema.FormName}' was invalid, The following integrity check results are failing: {SystemConstants.NEW_LINE_CHARACTER} {String.Join(SystemConstants.NEW_LINE_CHARACTER, failingCheckResultMessages)}");
        }
    }
}