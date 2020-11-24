using Microsoft.AspNetCore.Mvc;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.Booking
{
    public class FakeBookingProvider : IBookingProvider
    {
        public string ProviderName { get => "Fake"; }

        public Task<AvailabilityDayResponse> NextAvailability(AvailabilityRequest request)
        {
            var response = new AvailabilityDayResponse()
            {
                Date = DateTime.Now.AddDays(2),
                AppointmentTimes = new List<AppointmentTime>()
            };

            return Task.FromResult(response);
         }

        public Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request)
        {
            var availability = new List<AvailabilityDayResponse>();
            var todayDate = DateTime.Now;
            availability.Add(new AvailabilityDayResponse() 
            {
                Date = new DateTime(todayDate.Year, todayDate.Month, todayDate.Day),
                AppointmentTimes = new List<AppointmentTime>
                {
                    new AppointmentTime 
                    {
                        StartTime = new TimeSpan(7, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        Duration = new TimeSpan(10, 0, 0)
                    }
                }
            });

            return Task.FromResult(availability);
        }
        public Task<Guid> Reserve(BookingRequest request)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task<IActionResult> HasAvailability(AvailabilityRequest request)
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
