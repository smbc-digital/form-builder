using Microsoft.AspNetCore.Http;

namespace form_builder.Helpers.Session
{
    public class SessionHelper : ISessionHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetSessionGuid()
        {
            return _httpContextAccessor.HttpContext.Session.GetString("sessionGuid");
        }

        public void SetSessionGuid(string value)
        {
            _httpContextAccessor.HttpContext.Session.SetString("sessionGuid", value);
        }

        public void RemoveSessionGuid()
        {
             _httpContextAccessor.HttpContext.Session.Remove("sessionGuid");
        }
    }
}
