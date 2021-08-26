namespace form_builder.Helpers.Cookie
{
    public interface ICookieHelper
    {
        void AddCookie(string name, string value);
        void DeleteCookie(string key);
        string GetCookie(string key);
    }
}
