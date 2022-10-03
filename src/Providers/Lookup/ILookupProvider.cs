using form_builder.Models;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;

namespace form_builder.Providers.Lookup
{
    public interface ILookupProvider
    {
        string ProviderName { get; }
        Task<OptionsResponse> GetAsync(string url, string authToken);
        Task<List<AppointmentType>> GetAppointmentTypesAsync(string url, string authToken);
    }
}