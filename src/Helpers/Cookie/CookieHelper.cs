namespace form_builder.Helpers.Cookie;

public class CookieHelper(IHttpContextAccessor httpContextAccessor) : ICookieHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public void AddCookie(string key, string value)
    {
        var cookieOptions = new CookieOptions
        {
            Secure = true
        };

        _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
    }

    public void DeleteCookie(string key)
        => _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);

    public string GetCookie(string key)
        => _httpContextAccessor.HttpContext.Request.Cookies[key];
}