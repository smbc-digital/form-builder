using form_builder.Models;

namespace StockportWebapp.Utils;

public interface ICookieComplianceHelper
{
    void AddToCookies<T>(string slug, string cookieType);
    List<string> GetCookies<T>(string cookieType);
    List<T> PopulateCookies<T>(List<T> items, string cookieType);
    void RemoveAllFromCookies<T>(string cookieType);
    void RemoveFromCookies<T>(string slug, string cookieType);
    CookieConsentLevel GetCurrentCookieConsentLevel();
    bool HasCookieConsentBeenCollected();
    void RemoveCookie(string key);
    void RemoveCookiesStartingWith(string startKey);
}
