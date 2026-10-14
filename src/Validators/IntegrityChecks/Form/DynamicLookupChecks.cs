namespace form_builder.Validators.IntegrityChecks.Form;

public class DynamicLookupCheck(IWebHostEnvironment environment, IEnumerable<ILookupProvider> lookupProviders)
    : IFormSchemaIntegrityCheck
{
    public IntegrityCheckResult Validate(FormSchema schema)
    {
        IntegrityCheckResult result = new();

        List<IElement> elements = schema.Pages
            .SelectMany(page => page.Elements)
            .Where(element => !string.IsNullOrEmpty(element.Lookup) &&
                              element.Lookup.Equals(LookUpConstants.Dynamic))
            .ToList();

        if (elements.Count.Equals(0))
            return result;

        foreach (var element in elements)
        {
            if (element.Properties.LookupSources is null || !element.Properties.LookupSources.Any())
            {
                result.AddFailureMessage(
                    "Dynamic Lookup Check, " +
                    $"dynamic lookup with questionId {element.Properties.QuestionId} requires a LookupSource");

                return result;
            }

            if (!element.Properties.LookupSources
                    .Any(lookup => lookup.EnvironmentName
                        .Equals(environment.EnvironmentName, StringComparison.OrdinalIgnoreCase)))
            {
                result.AddFailureMessage(
                    $"The provided json has no Environment details for this:({environment.EnvironmentName}) Environment");
            }

            foreach (var env in element.Properties.LookupSources)
            {
                if (string.IsNullOrEmpty(env.EnvironmentName))
                    result.AddFailureMessage($"The provided json has no Environment Name");

                if (string.IsNullOrEmpty(env.Provider))
                    result.AddFailureMessage($"The provided json has no Provider Name");

                try
                {
                    lookupProviders.Get(env.Provider);
                }
                catch (Exception e)
                {
                    result.AddFailureMessage($"No specified Providers in form. Error Message {e.Message}");
                }

                if (string.IsNullOrEmpty(env.URL))
                    result.AddFailureMessage($"The provided json has no API URL to submit to");

                if (string.IsNullOrEmpty(env.AuthToken))
                    result.AddFailureMessage($"The provided json has no auth token for the API");

                if (!environment.IsEnvironment("local") &&
                    !env.EnvironmentName.Equals("local", StringComparison.OrdinalIgnoreCase) &&
                    !env.URL.StartsWith("https://"))
                {
                    result.AddFailureMessage($"SubmitUrl must start with https, in form.");
                }
            }
        }

        return result;
    }

    public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
}