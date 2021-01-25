using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models.Elements;
using form_builder.Services.MappingService.Entities;
using StockportGovUK.NetStandard.Models.Booking.Request;
using Address = StockportGovUK.NetStandard.Models.Addresses.Address;

namespace form_builder.Services.MappingService
{
    public interface IMappingService
    {
        Task<MappingEntity> Map(string sessionGuid, string form);
        Task<BookingRequest> MapBookingRequest(string sessionGuid, IElement bookingElement, Dictionary<string, dynamic> viewModel, string form);
        Task<string> MapAddress(string sessionGuid, string form);
    }
}
