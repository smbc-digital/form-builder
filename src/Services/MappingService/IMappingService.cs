﻿using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Services.MappingService.Entities;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;

namespace form_builder.Services.MappingService
{
    public interface IMappingService
    {
        Task<MappingEntity> Map(string sessionGuid, string form);
        Task<BookingRequest> MapBookingRequest(string sessionGuid, IElement bookingElement, Dictionary<string, dynamic> viewModel, string form);
        void MapAppointmentId(AppointmentType appointmentType, FormAnswers answers);
    }
}
