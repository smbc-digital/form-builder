using System.Diagnostics.CodeAnalysis;
using form_builder.Models;
using StockportWebapp.Utils;

namespace form_builder.Middleware;

/// <summary>
/// This is a one off piece of middleware to  ensure 
/// that no old cookies are retained from the previous consent scheme 
/// that we know require consent (Google Analytics, SiteImprove, Alerts dismissal, Old cookie consent)
/// </summary>
/// 
[ExcludeFromCodeCoverage]
public class CookiesComplianceMiddleware(RequestDelegate next, ICookieComplianceHelper cookiesHelper)
{
    private readonly RequestDelegate _next = next;
    private readonly ICookieComplianceHelper _cookiesHelper = cookiesHelper;

    public Task Invoke(HttpContext httpContext)
    {
        // Remove legacy cookie consent
        _cookiesHelper.RemoveCookie("wpcc");

        if (!_cookiesHelper.HasCookieConsentBeenCollected())
        {
            RemoveFunctionalCookies();
            RemoveTrackingCookies();

            return _next(httpContext);
        }

        CookieConsentLevel consentLevels = _cookiesHelper.GetCurrentCookieConsentLevel();

        if (!consentLevels.Functionality)
            RemoveFunctionalCookies();

        if (!consentLevels.Tracking)
            RemoveTrackingCookies();

        if (!consentLevels.Targetting)
            RemoveTargettingCookies();

        return _next(httpContext);
    }

    private void RemoveFunctionalCookies()
    {
        _cookiesHelper.RemoveCookie("alerts");
        _cookiesHelper.RemoveCookie("favourites");
    }

    private void RemoveTrackingCookies()
    {
        _cookiesHelper.RemoveCookiesStartingWith("_ga");
        _cookiesHelper.RemoveCookie("_gat");
        _cookiesHelper.RemoveCookie("_gid");
        _cookiesHelper.RemoveCookie("nmstat");
        _cookiesHelper.RemoveCookie("hstc");
        _cookiesHelper.RemoveCookie("unam");
        _cookiesHelper.RemoveCookie("hsfirstvisit");
        _cookiesHelper.RemoveCookie("hubspotuk");
        _cookiesHelper.RemoveCookie("siteimproveses");
        _cookiesHelper.RemoveCookie("ga");
    }

    private void RemoveTargettingCookies()
    {
        // No targetting cookies currently set  
    }
}