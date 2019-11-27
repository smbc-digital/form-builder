namespace form_builder.Helpers.Session
{
    public interface ISessionHelper
    {
        string GetSessionGuid();

        void RemoveSessionGuid();

        void SetSessionGuid(string value);
    }
}
