using form_builder.Constants;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class BaseUrlCheck : IFormSchemaIntegrityCheck
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BaseUrlCheck(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (string.IsNullOrEmpty(schema.BaseURL))
            {
                result.AddFailureMessage(
                    "FormSchema BaseURL Check, " +
                    "BaseUrl property cannot be null or empty and needs to be the same as: " +
                    $"base request URL {_httpContextAccessor.HttpContext.Request.Path}");

                return result;
            }

            if (_httpContextAccessor.HttpContext.Request.Path.Value.StartsWith("/Preview") ||
                _httpContextAccessor.HttpContext.Request.Path.Value.StartsWith("/view") ||
                _httpContextAccessor.HttpContext.Request.Path.Value.StartsWith($"/{PreviewConstants.PREVIEW_MODE_PREFIX}"))
                return result;

            if (!_httpContextAccessor.HttpContext.Request.Path.Value.Contains($"/{schema.BaseURL.ToLower()}"))
            {
                result.AddFailureMessage(
                    "FormSchema BaseURL Check, " +
                    "BaseUrl property within form schema needs to be the same as:" +
                    $" base request URL {_httpContextAccessor.HttpContext.Request.Path}");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
