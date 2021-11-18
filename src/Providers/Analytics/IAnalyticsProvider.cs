using System.Threading.Tasks;
using form_builder.Providers.Analytics.Request;

namespace form_builder.Providers.Analytics
{
    public interface IAnalyticsProvider
    {
        string ProviderName { get; }
        Task RaiseEventAsync(AnalyticsEventRequest request);
    }
}
