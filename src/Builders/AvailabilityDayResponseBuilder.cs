using System;
using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Booking.Response;

namespace form_builder.Builders
{
    public class AvailabilityDayResponseBuilder
    {
        private List<AvailabilityDayResponse> _response = new List<AvailabilityDayResponse>();

        public List<AvailabilityDayResponse> Build() => _response;

        public AvailabilityDayResponseBuilder WithDay(DateTime date, int appointments)
        {
            var appointmentTimes = new List<AppointmentTime>();
            for (int i = 0; i < appointments; i++)
            {
                appointmentTimes.Add(new AppointmentTime
                {
                    StartTime = new TimeSpan(7, 0, 0),
                    EndTime = new TimeSpan(17, 30, 0)
                });
            }

            _response.Add(new AvailabilityDayResponse {
                AppointmentTimes = appointmentTimes,
                Date = date
            });

            return this;
        }
    }
}