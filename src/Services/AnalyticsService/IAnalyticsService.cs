namespace form_builder.Services.AnalyticsService;

public interface IAnalyticsService
{
    void RaiseEvent(string form, EAnalyticsEventType eventType);
}