using form_builder.Builders;
using form_builder.Exceptions;
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
                        .WithDay(new DateTime(2020, request.StartDate.Month, 13), 1)
                        .WithDay(new DateTime(2020, request.StartDate.Month, 15), 1)
                        .WithDay(new DateTime(2020, request.StartDate.Month, 22), 1)
                        .WithDay(new DateTime(2020, request.StartDate.Month, 23), 1)
                        .Build();
                    break;
                case 12:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2020, request.StartDate.Month, 15), 1)
                        .WithDay(new DateTime(2020, request.StartDate.Month, 12), 1)
                        .WithDay(new DateTime(2020, request.StartDate.Month, 13), 1)
                        .WithDay(new DateTime(2020, request.StartDate.Month, 20), 1)
                        .Build();
                    break;
                case 1:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, request.StartDate.Month, 1), 1)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 4), 1)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 9), 1)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 22), 1)
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
    }
}
