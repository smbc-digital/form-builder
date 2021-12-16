using System;
using System.Collections.Generic;
using form_builder.Models;
using StockportGovUK.NetStandard.Models.Booking.Request;

namespace form_builder_tests.Builders
{
    public class AppointmentTypeBuilder
    {
        private string _environment = "test";
        private Guid _appointmentId = Guid.NewGuid();
        private string _appointmentIdKey = "00000000-0000-0000-0000-000000000000";
        private List<BookingResource> _optionalResources = new List<BookingResource>();

        public AppointmentType Build() => new AppointmentType
        {
            Environment = _environment,
            AppointmentId = _appointmentId,
            AppointmentIdKey = _appointmentIdKey,
            OptionalResources = _optionalResources
        };

        public AppointmentTypeBuilder WithAppointmentId(Guid value)
        {
            _appointmentId = value;

            return this;
        }

        public AppointmentTypeBuilder WithAppointmentIdKey(string value)
        {
            _appointmentIdKey = value;

            return this;
        }

        public AppointmentTypeBuilder WithEnvironment(string value)
        {
            _environment = value;

            return this;
        }

        public AppointmentTypeBuilder WithOptionalResource(BookingResource resource)
        {
            if (_optionalResources is null)
                _optionalResources = new List<BookingResource>();

            _optionalResources.Add(resource);

            return this;
        }
    }
}