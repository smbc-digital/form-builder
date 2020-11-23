using form_builder.Services.BookingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Providers.Booking
{
    public class BookingApiProvider : IBookingProvider
    {
        private readonly IBookingService _bookingService;
        public BookingApiProvider(IBookingService bookingService) => _bookingService = bookingService;

        public Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<IActionResult> HasAvailability(AvailabilityRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<IActionResult> NextAvailability(AvailabilityRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<IActionResult> Reserve(BookingRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<IActionResult> Confirm(ConfirmationRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<IActionResult> Cancel(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
