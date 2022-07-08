using System.Threading.Tasks;
using form_builder.Services.MappingService.Entities;
using StockportGovUK.NetStandard.Models.Civica.Pay.Notifications;

namespace form_builder.Services.PayService
{
    public interface IPayService
    {
        Task<string> ProcessPayment(MappingEntity mappingEntity, string form, string path, string reference, string sessionGuid);
        Task<string> ProcessPaymentResponse(string form, string responseCode, string reference);
        string LogPayment(string form, NotificationMessage notification);
    }
}
