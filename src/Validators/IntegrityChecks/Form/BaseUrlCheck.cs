namespace form_builder.Validators.IntegrityChecks.Form;

public class BaseUrlCheck(IHttpContextAccessor httpContextAccessor) : IFormSchemaIntegrityCheck
{
    public IntegrityCheckResult Validate(FormSchema schema)
    {
        IntegrityCheckResult result = new();

        if (string.IsNullOrEmpty(schema.BaseURL))
        {
            result.AddFailureMessage(
                "FormSchema BaseURL Check, " +
                "BaseUrl property cannot be null or empty and needs to be the same as: " +
                $"base request URL {httpContextAccessor.HttpContext.Request.Path}");

            return result;
        }

        if (httpContextAccessor.HttpContext.Request.Path.Value.StartsWith("/Preview") ||
            httpContextAccessor.HttpContext.Request.Path.Value.StartsWith("/view") ||
            httpContextAccessor.HttpContext.Request.Path.Value.StartsWith($"/{PreviewConstants.PREVIEW_MODE_PREFIX}"))
            return result;

        if (!httpContextAccessor.HttpContext.Request.Path.Value.Contains($"/{schema.BaseURL.ToLower()}"))
        {
            result.AddFailureMessage(
                "FormSchema BaseURL Check, " +
                "BaseUrl property within form schema needs to be the same as:" +
                $" base request URL {httpContextAccessor.HttpContext.Request.Path}");
        }

        return result;
    }

    public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
}