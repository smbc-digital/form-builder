using form_builder.Models;

namespace form_builder.Restrictions
{
    public class RefererRestriction : IFormAccessRestriction
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public RefererRestriction(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public bool IsRestricted(FormSchema baseForm)
        {
            if (baseForm.FormAccessReferrers is null || !baseForm.FormAccessReferrers.Any())
                return false;
            
            if (string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Headers.Referer) || !_httpContextAccessor.HttpContext.Request.Headers.Referer.Any())
                return true;

            if (!baseForm.FormAccessReferrers.Any(referer => _httpContextAccessor.HttpContext.Request.Headers.Referer.ToString().Contains(referer)))
                return true;

            return false;
        }
    }
}