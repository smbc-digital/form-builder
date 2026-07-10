namespace form_builder.Validators.IntegrityChecks.Form;

public class ValidateActionCheck(IWebHostEnvironment environment) : IFormSchemaIntegrityCheck
{
    public IntegrityCheckResult Validate(FormSchema schema)
    {
        IntegrityCheckResult result = new();

        List<IAction> actions = schema.FormActions
            .Where(formAction => formAction.Type.Equals(EActionType.Validate))
            .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(action => action.Type.Equals(EActionType.Validate)))
            .ToList();

        if (actions.Count.Equals(0))
            return result;

        actions.ForEach(action =>
        {
            PageActionSlug slug = action.Properties.PageActionSlugs
                .FirstOrDefault(slug => slug.Environment
                    .Equals(environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase));

            if (slug is null)
                result.AddFailureMessage($"Validate Action Check, Validate there is no PageActionSlug for {environment.EnvironmentName}");

            if (string.IsNullOrEmpty(slug.URL))
                result.AddFailureMessage("Validate Action Check, Validate action type does not contain a url");

            if (action.Properties.HttpActionType.Equals(EHttpActionType.Unknown))
                result.AddFailureMessage("Validate Action Check, Validate action type does not contain 'Unknown'");
        });

        return result;
    }

    public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
}