using form_builder.Models;

namespace form_builder.Restrictions
{
    public class KeyFormAccessRestriction : IFormAccessRestriction
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public KeyFormAccessRestriction(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public bool IsRestricted(FormSchema baseForm)
        {
            if (string.IsNullOrEmpty(baseForm.FormAccessKey) && string.IsNullOrEmpty(baseForm.FormAccessKeyName))
                return false;

            var context = _httpContextAccessor.HttpContext;
            if (!context.Request.Query.Any(keyValuePair => keyValuePair.Key.Equals(baseForm.FormAccessKeyName)))
                return true;
            
            var keyValuePair = context.Request.Query.Single(keyValuePair => keyValuePair.Key.Equals(baseForm.FormAccessKeyName));

            return !keyValuePair.Value.Equals(baseForm.FormAccessKey);
        }
    }
}