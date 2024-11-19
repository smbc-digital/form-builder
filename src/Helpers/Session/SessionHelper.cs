namespace form_builder.Helpers.Session;

public class SessionHelper : ISessionHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionHelper(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    // Remove
    public ISession GetSession() => _httpContextAccessor.HttpContext.Session;

    public string GetBrowserSessionId() => _httpContextAccessor.HttpContext.Session.Id;

    // Remove
    public string GetSessionGuid() => _httpContextAccessor.HttpContext.Session.GetString("sessionGuid");

    // Remove
    public void SetSessionGuid(string value) => _httpContextAccessor.HttpContext.Session.SetString("sessionGuid", value);

    public void SetSessionFormName(string key, string value) => _httpContextAccessor.HttpContext.Session.SetString(key, value);

    public string GetSessionFormName(string key) => _httpContextAccessor.HttpContext.Session.GetString(key);

    // Remove
    public void RemoveSessionGuid() => _httpContextAccessor.HttpContext.Session.Remove("sessionGuid");

    // Remove
    public void SetSessionForm(string value) => _httpContextAccessor.HttpContext.Session.SetString("sessionForm", value);

    // Remove
    public string GetSessionForm() =>  _httpContextAccessor.HttpContext.Session.GetString("sessionForm");

    // Remove
    public void RemoveSessionForm() => _httpContextAccessor.HttpContext.Session.Remove("sessionForm");

    // Remove
    public void Clear()
    {
        RemoveSessionForm();
        RemoveSessionGuid();
        _httpContextAccessor.HttpContext.Session.Clear();
    }

    // Remove
    public void Set(string guid, string form)
    {
        SetSessionGuid(guid);
        SetSessionForm(form);
    }
}