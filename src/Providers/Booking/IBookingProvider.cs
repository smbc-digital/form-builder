using Microsoft.AspNetCore.Mvc;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Providers.Booking
{
    public interface IBookingProvider
    {
        string ProviderName { get; }

        Task<AvailabilityDayResponse> NextAvailability(AvailabilityRequest request);
        Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request);
        Task<Guid> Reserve(BookingRequest request);

        Task<IActionResult> HasAvailability(AvailabilityRequest request);
        Task<IActionResult> Cancel(Guid id);
        Task<IActionResult> Confirm(ConfirmationRequest request);

    }
}
