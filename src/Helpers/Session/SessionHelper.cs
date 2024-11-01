namespace form_builder.Helpers.Session;

public class SessionHelper : ISessionHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionHelper(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public ISession GetSession() => _httpContextAccessor.HttpContext.Session;

    public string GetBrowserSessionId() => _httpContextAccessor.HttpContext.Session.Id;

    public string GetSessionGuid() => _httpContextAccessor.HttpContext.Session.GetString("sessionGuid");

    public void SetSessionGuid(string value) => _httpContextAccessor.HttpContext.Session.SetString("sessionGuid", value);

    public void RemoveSessionGuid() => _httpContextAccessor.HttpContext.Session.Remove("sessionGuid");

    public void SetSessionForm(string value) => _httpContextAccessor.HttpContext.Session.SetString("sessionForm", value);

    public string GetSessionForm() =>  _httpContextAccessor.HttpContext.Session.GetString("sessionForm");

    public void RemoveSessionForm() => _httpContextAccessor.HttpContext.Session.Remove("sessionForm");        

    public void Clear()
    {
        RemoveSessionForm();
        RemoveSessionGuid();
        _httpContextAccessor.HttpContext.Session.Clear();
    }

    public void Set(string guid, string form)
    {
        SetSessionGuid(guid);
        SetSessionForm(form);
    }
}