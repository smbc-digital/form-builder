using form_builder.Builders;
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
                Date = DateTime.Now.AddMonths(1),
                AppointmentTimes = new List<AppointmentTime>
                {
                    new AppointmentTime
                    {
                        StartTime = new TimeSpan(7, 0, 0),
                        EndTime = new TimeSpan(17, 30, 0)
                    }
                }
            };

            return Task.FromResult(response);
        }

        public Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request)
        {
            var todayDate = DateTime.Now;
            var response = new List<AvailabilityDayResponse>();
            switch (request.StartDate.Month)
            {
                case 11:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2020, 10, 13), 1)
                        .WithDay(new DateTime(2020, 10, 15), 1)
                        .WithDay(new DateTime(2020, 10, 22), 1)
                        .WithDay(new DateTime(2020, 10, 23), 1)
                        .Build();
                    break;
                case 12:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2020, 11, 15), 1)
                        .WithDay(new DateTime(2020, 11, 12), 1)
                        .WithDay(new DateTime(2020, 11, 13), 1)
                        .WithDay(new DateTime(2020, 11, 20), 1)
                        .Build();
                    break;
                case 1:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2020, 12, 1), 1)
                        .WithDay(new DateTime(2020, 12, 4), 1)
                        .WithDay(new DateTime(2020, 12, 9), 1)
                        .WithDay(new DateTime(2020, 12, 22), 1)
                        .Build();
                    break;
                default:
                    break;
            }

            return Task.FromResult(response);
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
