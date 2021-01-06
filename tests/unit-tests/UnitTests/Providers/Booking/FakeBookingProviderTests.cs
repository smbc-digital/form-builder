using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Exceptions;
using form_builder.Providers.Booking;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Booking
{
    public class FakeBookingProviderTests
    {
        private readonly FakeBookingProvider _bookigProvider;

        public FakeBookingProviderTests()
        {
            _bookigProvider = new FakeBookingProvider();
        }

        [Fact]
        public async Task NextAvailability_ShouldReturnDayResponse()
        {
            var request = new AvailabilityRequest{ AppointmentId = Guid.NewGuid() };

            var result = await _bookigProvider.NextAvailability(request);

            Assert.IsType<AvailabilityDayResponse>(result);
        }

        [Fact]
        public async Task NextAvailability_Should_Throw_BookingNoAvailabilityException_OnEmptyGuid()
        {
            var request = new AvailabilityRequest{ AppointmentId = Guid.Parse("00000000-0000-0000-0000-000000000001") };
            var result = await Assert.ThrowsAsync<BookingNoAvailabilityException>(() => _bookigProvider.NextAvailability(request));

            Assert.Equal("FakeProvider, no available appointments", result.Message);
        }


        [Fact]
        public async Task GetAvailability_ShouldReturn_ListOfDayResponse()
        {
            var request = new AvailabilityRequest();

            var result = await _bookigProvider.GetAvailability(request);

            Assert.IsType<List<AvailabilityDayResponse>>(result);
        }

        [Fact]
        public async Task Reserve_ShouldReturn_Guid()
        {
            var request = new BookingRequest();

            var result = await _bookigProvider.Reserve(request);

            Assert.IsType<Guid>(result);
        }

        [Fact]
        public async Task GetLocation_ShouldReturnStringLocation()
        {
            var request = new LocationRequest { AppointmentId = Guid.NewGuid() };

            var result = await _bookigProvider.GetLocation(request);

            Assert.IsType<string>(result);
        }
    }
}
