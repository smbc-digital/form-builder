using form_builder.Models;

namespace form_builder.Restrictions
{
    public class KeyFormAccessRestriction : IFormAccessRestriction
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public KeyFormAccessRestriction(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsRestricted(FormSchema baseForm)
        {
            if(!string.IsNullOrEmpty(baseForm.Key) && !string.IsNullOrEmpty(baseForm.KeyName))
            {
                var context = _httpContextAccessor.HttpContext;
                if(!_httpContextAccessor.HttpContext.Request.Query.Any(KeyValuePair => KeyValuePair.Key == baseForm.KeyName))
                    return true;
                
                var keyValuePair = _httpContextAccessor.HttpContext.Request.Query.SingleOrDefault(KeyValuePair => KeyValuePair.Key == baseForm.KeyName);
                if(!keyValuePair.Value.Equals(baseForm.Key))
                    return true;
            }

            return false;
        }
    }
}