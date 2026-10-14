namespace form_builder.Validators.IntegrityChecks.Form;

public class RetrieveExternalActionsCheck(IWebHostEnvironment environment) : IFormSchemaIntegrityCheck
{
    public IntegrityCheckResult Validate(FormSchema schema)
    {
        IntegrityCheckResult result = new();

        List<IAction> actions = schema.FormActions
            .Where(formAction => formAction.Type.Equals(EActionType.RetrieveExternalData))
            .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(pageAction => pageAction.Type.Equals(EActionType.RetrieveExternalData))).ToList();

        if (actions.Count.Equals(0))
            return result;

        actions.ForEach(action =>
        {
            PageActionSlug slug = action.Properties.PageActionSlugs
                .FirstOrDefault(slugs => slugs.Environment
                    .Equals(environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase));

            if (slug is null)
            {
                result.AddFailureMessage(
                    "Retrieve External Data Action, " +
                    $"there is no PageActionSlug for environment '{environment.EnvironmentName}'");
            }
            else
            {
                if (string.IsNullOrEmpty(slug.URL))
                    result.AddFailureMessage("Retrieve External Data Action, action type does not contain a url");
            }

            if (string.IsNullOrEmpty(action.Properties.TargetQuestionId))
                result.AddFailureMessage("Retrieve External Data Action, action type does not contain a TargetQuestionId");
        });

        return result;
    }

    public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
}