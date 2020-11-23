using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Providers.Booking
{
    public interface IBookingProvider
    {
        Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request);

        Task<IActionResult> HasAvailability(AvailabilityRequest request);

        Task<IActionResult> NextAvailability(AvailabilityRequest request);

        Task<IActionResult> Cancel(Guid id);

        Task<IActionResult> Confirm(ConfirmationRequest request);

        Task<IActionResult> Reserve(BookingRequest request);
    }
}
