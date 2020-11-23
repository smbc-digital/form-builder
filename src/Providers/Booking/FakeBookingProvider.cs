﻿using form_builder.Services.BookingService;
using Microsoft.AspNetCore.Mvc;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Date = DateTime.Now,
                AppointmentTimes = new List<AppointmentTime>()
            };

            return Task.FromResult(response);
        }
        public Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request)
        {
            var availability = new List<AvailabilityDayResponse>();
            availability.Add(new AvailabilityDayResponse() 
            {
                Date = DateTime.Now,
                AppointmentTimes = new List<AppointmentTime>()
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
