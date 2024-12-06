namespace form_builder.Helpers.Session
{
    public interface ISessionHelper
    {
        ISession GetSession();
        string GetBrowserSessionId();
        string GetSessionGuid();
        void RemoveSessionGuid();
        void SetSessionGuid(string value);
        void SetSessionFormName(string key, string value);
        string GetSessionFormName(string key);
        void SetSessionForm(string value);
        string GetSessionForm();
        void RemoveSessionForm();
        void Clear();
        void Set(string guid, string form);
    }
}
