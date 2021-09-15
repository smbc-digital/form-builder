
using form_builder.Providers.Analytics.Request;

namespace form_builder.Services.AnalyticsService
{
    public interface IAnalyticsService
    {
        void RaiseEvent(string form, EAnalyticsEventType eventType);
    }
}