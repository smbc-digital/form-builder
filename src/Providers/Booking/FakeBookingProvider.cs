using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Exceptions;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;

namespace form_builder.Providers.Booking
{
    public class FakeBookingProvider : IBookingProvider
    {
        public string ProviderName { get => "Fake"; }
        private static string BOOKING_UI_TEST => "00000000-0000-0000-0000-000000000002";
        private static string BOOKING_WITH_NO_AVAILABILITY => "00000000-0000-0000-0000-000000000001";
        private static string BOOKING_NON_FULL_DAY_APPOINTMENT => "00000000-0000-0000-0000-000000000003";
        private static string BOOKING_CANNOT_BE_CANCELLED => "00000000-0000-0000-0000-000000000004";
        private static string BOOKING_WITH_ERROR_ON_CANCEL => "00000000-0000-0000-0000-000000000005";

        public Task<AvailabilityDayResponse> NextAvailability(AvailabilityRequest request)
        {
            if (request.AppointmentId.Equals(Guid.Parse(BOOKING_WITH_NO_AVAILABILITY)))
                throw new BookingNoAvailabilityException("FakeProvider, no available appointments");

            if (request.AppointmentId.Equals(Guid.Parse(BOOKING_UI_TEST)))
                return Task.FromResult(new AvailabilityDayResponse()
                {
                    Date = new DateTime(2021, 2, 1),
                    AppointmentTimes = new List<AppointmentTime>
                    {
                        new AppointmentTime
                        {
                            StartTime = new TimeSpan(7, 0, 0),
                            EndTime = new TimeSpan(17, 30, 0)
                        }
                    }
                });

            if (request.AppointmentId.Equals(Guid.Parse(BOOKING_NON_FULL_DAY_APPOINTMENT)))
                return NextAvailability_ForNonFullDayAppointment(request);

            return NextAvailability_ForFullDayAppointment(request);
        }

        public Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request)
        {
            if (request.AppointmentId.Equals(Guid.Parse(BOOKING_UI_TEST)))
                return Task.FromResult(new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, 2, 1), 1, true)
                        .Build());

            if (request.AppointmentId.Equals(Guid.Parse(BOOKING_NON_FULL_DAY_APPOINTMENT)))
                return GetAvailability_ForNonFullDayAppointment(request);

            return GetAvailability_ForFullDayAppointment(request);
        }

        public Task<Guid> Reserve(BookingRequest request)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        private Task<AvailabilityDayResponse> NextAvailability_ForFullDayAppointment(AvailabilityRequest request) =>
        Task.FromResult(new AvailabilityDayResponse()
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
        });

        private Task<AvailabilityDayResponse> NextAvailability_ForNonFullDayAppointment(AvailabilityRequest request) =>
        Task.FromResult(new AvailabilityDayResponse()
        {
            Date = DateTime.Now.AddMonths(1),
            AppointmentTimes = new List<AppointmentTime>
            {
                new AppointmentTime
                {
                    StartTime = new TimeSpan(11, 0, 0),
                    EndTime = new TimeSpan(13, 30, 0)
                }
            }
        });

        private Task<List<AvailabilityDayResponse>> GetAvailability_ForFullDayAppointment(AvailabilityRequest request)
        {
            var response = new List<AvailabilityDayResponse>();
            switch (request.StartDate.Month)
            {
                case 3:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, request.StartDate.Month, 13), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 15), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 22), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 23), 1, true)
                        .Build();
                    break;
                case 4:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, request.StartDate.Month, 15), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 12), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 13), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 20), 1, true)
                        .Build();
                    break;
                case 5:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, request.StartDate.Month, 1), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 4), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 9), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 22), 1, true)
                        .Build();
                    break;
                case 6:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, request.StartDate.Month, 4), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 5), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 6), 1, true)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 7), 1, true)
                        .Build();
                    break;
                default:
                    break;
            }

            return Task.FromResult(response);
        }

        private Task<List<AvailabilityDayResponse>> GetAvailability_ForNonFullDayAppointment(AvailabilityRequest request)
        {
            var response = new List<AvailabilityDayResponse>();
            switch (request.StartDate.Month)
            {
                case 2:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, request.StartDate.Month, 13), 3, false)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 15), 6, false)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 22), 2, false)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 23), 3, false)
                        .Build();
                    break;
                case 3:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, request.StartDate.Month, 15), 3, false)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 12), 1, false)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 13), 2, false)
                        .WithDay(new DateTime(2021, request.StartDate.Month, 20), 8, false)
                        .Build();
                    break;
                case 4:
                    response = new AvailabilityDayResponseBuilder()
                        .WithDay(new DateTime(2021, request.StartDate.Month, 1), 0, false)
                        .Build();
                    break;
                default:
                    break;
            }

            return Task.FromResult(response);
        }

        public Task<string> GetLocation(LocationRequest request)
        {
            return Task.FromResult("Test, Test St, Stockport SK1 3UR");
        }

        public Task<BookingInformationResponse> GetBooking(Guid bookingId)
        {
            if (bookingId.Equals(Guid.Parse(BOOKING_CANNOT_BE_CANCELLED)))
            {
                return Task.FromResult(new BookingInformationResponse
                {
                    Date = new DateTime(2021, 4, 13),
                    CanCustomerCancel = false,
                    StartTime = new TimeSpan(1,1,1),
                    EndTime = new TimeSpan(2,1, 1)
                });
            }

            return Task.FromResult(new BookingInformationResponse
            {
                Date = new DateTime(2021, 3, 13),
                CanCustomerCancel = true,
                StartTime = new TimeSpan(2, 1, 1),
                EndTime = new TimeSpan(23, 1, 1)
            });
        }

        public async Task Cancel(Guid bookingId)
        {
            if(bookingId.Equals(Guid.Parse(BOOKING_WITH_ERROR_ON_CANCEL)))
                throw new Exception("FakeBookingProvider::Cancel, Booking faked with fake exception");

            _ = await Task.FromResult("");
        }

        public async Task Confirm(ConfirmationRequest request)
        {
            _ = await Task.FromResult("");
        }
    }
}
