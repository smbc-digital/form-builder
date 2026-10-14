namespace form_builder.Restrictions;

public class RefererRestriction(IHttpContextAccessor httpContextAccessor) : IFormAccessRestriction
{
    public bool IsRestricted(FormSchema baseForm)
    {
        if (baseForm.FormAccessReferrers is null || !baseForm.FormAccessReferrers.Any())
            return false;
            
        if (string.IsNullOrEmpty(httpContextAccessor.HttpContext.Request.Headers.Referer) || !httpContextAccessor.HttpContext.Request.Headers.Referer.Any())
            return true;

        if (!baseForm.FormAccessReferrers.Any(referer => httpContextAccessor.HttpContext.Request.Headers.Referer.ToString().Contains(referer)))
            return true;

        return false;
    }
}