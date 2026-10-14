namespace form_builder.Restrictions;

public class KeyFormAccessRestriction(IHttpContextAccessor httpContextAccessor,
    IOptions<QAFormAccessTokenConfiguration> qaFormAccessToken)
    : IFormAccessRestriction
{
    private readonly QAFormAccessTokenConfiguration _qaFormAccessToken = qaFormAccessToken.Value;

    public bool IsRestricted(FormSchema baseForm)
    {
        if (string.IsNullOrEmpty(baseForm.FormAccessKey) && string.IsNullOrEmpty(baseForm.FormAccessKeyName))
            return false;

        var context = httpContextAccessor.HttpContext;
        if (!context.Request.Query.Any(keyValuePair => keyValuePair.Key.Equals(baseForm.FormAccessKeyName, StringComparison.OrdinalIgnoreCase)))
            return true;
            
        var keyValuePair = context.Request.Query.Single(keyValuePair => keyValuePair.Key.Equals(baseForm.FormAccessKeyName, StringComparison.OrdinalIgnoreCase));

        if (keyValuePair.Value.Equals(_qaFormAccessToken.AccessKey))
            return false;

        return !keyValuePair.Value.Equals(baseForm.FormAccessKey);
    }
}