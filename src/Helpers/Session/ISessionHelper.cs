namespace form_builder.Helpers.Session;

public interface ISessionHelper
{
    string GetBrowserSessionId();
    void SetSessionFormName(string key, string value);
    string GetSessionFormName(string key);
}