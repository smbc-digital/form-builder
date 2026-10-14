namespace form_builder.Helpers.Session;

public class SessionHelper(IHttpContextAccessor httpContextAccessor) : ISessionHelper
{
    public string GetBrowserSessionId() => httpContextAccessor.HttpContext.Session.Id;

    public void SetSessionFormName(string key, string value) => httpContextAccessor.HttpContext.Session.SetString(key, value);

    public string GetSessionFormName(string key) => httpContextAccessor.HttpContext.Session.GetString(key);
}