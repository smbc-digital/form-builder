using System;
using System.Collections.Generic;
using form_builder.Models;
using StockportGovUK.NetStandard.Models.Booking.Request;

namespace form_builder.Builders
{
    public class AppointmentTypeBuilder
    {
        private string _environment = "test";
        private Guid _appointmentId = Guid.NewGuid();
        private List<BookingResource> _optionalResources = new List<BookingResource>();

        public AppointmentType Build() => new AppointmentType
        {
            Environment = _environment,
            AppointmentId = _appointmentId,
            OptionalResources = _optionalResources
        };

        public AppointmentTypeBuilder WithAppointmentId(Guid value)
        {
            _appointmentId = value;

            return this;
        }

        public AppointmentTypeBuilder WithEnvironment(string value)
        {
            _environment = value;

            return this;
        }

        public AppointmentTypeBuilder WithOptionalResource(BookingResource resource)
        {
            if(_optionalResources == null)
                _optionalResources = new List<BookingResource>();

            _optionalResources.Add(resource);

            return this;
        }
    }
}