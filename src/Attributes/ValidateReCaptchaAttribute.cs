namespace form_builder.Attributes;

public class ValidateReCaptchaAttribute(IGateway gateway,
    IOptions<ReCaptchaConfiguration> configuration,
    ILogger<ValidateReCaptchaAttribute> logger)
    : ActionFilterAttribute
{
    private readonly ReCaptchaConfiguration _configuration = configuration.Value;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var formData = (IDictionary<string, string[]>)context.ActionArguments["formData"];
        if (context.ActionArguments["path"].Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH) && formData is not null && formData.ContainsKey("Submit"))
        {
            await DoReCaptchaValidation(context);
        }

        await base.OnActionExecutionAsync(context, next);
    }

    private static void AddModelError(ActionExecutingContext context, string error)
    {
        context.ModelState.AddModelError(ReCaptchaConstants.ReCaptchaModelErrorKey, error);
    }

    private async Task DoReCaptchaValidation(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.HasFormContentType)
        {
            AddModelError(context, "No reCaptcha Token Found");
            logger.LogWarning("ValidateReCaptchaAttribute:: DoReCaptchaValidation:: No reCaptcha Token Found");
            return;
        }

        var token = context.HttpContext.Request.Form[ReCaptchaConstants.RecaptchaResponseTokenKey];

        if (string.IsNullOrWhiteSpace(token))
        {
            AddModelError(context, "Verify you are not a robot by selecting the verification box above the \"submit\" button");
        }
        else
        {
            await ValidateRecaptcha(context, token);
        }
    }

    private async Task ValidateRecaptcha(ActionExecutingContext context, string token)
    {
        var request = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("secret", _configuration.AuthToken),
            new KeyValuePair<string, string>("response", token)
        });

        var response = await gateway.PostAsync(_configuration.ApiVerificationEndpoint, request, false);

        if (response.Content is null)
        {
            AddModelError(context, "Unable To Read Response From Server");
            logger.LogWarning("ValidateReCaptchaAttribute:: ValidateReCaptcha:: Unable To Read Response From Server");
        }
        else if (!response.IsSuccessStatusCode)
        {
            AddModelError(context, "Invalid reCaptcha");
            logger.LogWarning("ValidateReCaptchaAttribute:: ValidateReCaptcha:: Invalid reCaptcha");
        }
    }
}