using Microsoft.AspNetCore.Http;

namespace form_builder.Helpers.Cookie
{
    public class CookieHelper : ICookieHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CookieHelper(IHttpContextAccessor httpContextAccessor) 
            => _httpContextAccessor = httpContextAccessor;

        public void AddCookie(string key, string value)
        {
            var cookieOptions = new CookieOptions
            {
                Secure = true
            };
            
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }

        public void DeleteCookie(string key)
            =>_httpContextAccessor.HttpContext.Response.Cookies.Delete(key);

        public string GetCookie(string key) 
            => _httpContextAccessor.HttpContext.Request.Cookies[key];
    }
}