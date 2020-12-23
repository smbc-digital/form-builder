using System;
using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Booking.Response;

namespace form_builder.Builders
{
    public class AvailabilityDayResponseBuilder
    {
        private List<AvailabilityDayResponse> _response = new List<AvailabilityDayResponse>();

        public List<AvailabilityDayResponse> Build() => _response;

        public AvailabilityDayResponseBuilder WithDay(DateTime date, int appointments, bool IsFullDayAppointment)
        {
            var appointmentTimes = new List<AppointmentTime>();
            for (int i = 0; i < appointments; i++)
            {
                var startTime = StartTime(IsFullDayAppointment);
                appointmentTimes.Add(new AppointmentTime
                {
                    StartTime = startTime,
                    EndTime = EndTime(IsFullDayAppointment, startTime)
                });
            }

            _response.Add(new AvailabilityDayResponse {
                AppointmentTimes = appointmentTimes,
                Date = date
            });

            return this;
        }

        private TimeSpan StartTime(bool isFullDay) => isFullDay ? new TimeSpan(7, 0, 0) : new TimeSpan(new Random().Next(9, 17), 0, 0);
        private TimeSpan EndTime(bool isFullDay, TimeSpan startTime) => isFullDay ? new TimeSpan(17, 30, 0) : new TimeSpan(new Random().Next(startTime.Hours, 18), 0, 0);
    }
}