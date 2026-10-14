namespace form_builder.Helpers.Cookie;

public class CookieHelper(IHttpContextAccessor httpContextAccessor) : ICookieHelper
{
    public void AddCookie(string key, string value)
    {
        var cookieOptions = new CookieOptions
        {
            Secure = true
        };

        httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
    }

    public void DeleteCookie(string key)
        => httpContextAccessor.HttpContext.Response.Cookies.Delete(key);

    public string GetCookie(string key)
        => httpContextAccessor.HttpContext.Request.Cookies[key];
}