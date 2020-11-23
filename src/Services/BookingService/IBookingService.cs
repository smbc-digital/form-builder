using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Services.BookingService
{
    public interface IBookingService
    {
        Task<List<AvailabilityDayResponse>> GetAvailability();
    }
}
