using System;
using System.Threading.Tasks;
using form_builder.Exceptions;
using form_builder.Providers.Booking;
using Moq;
using StockportGovUK.NetStandard.Gateways.BookingService;
using StockportGovUK.NetStandard.Gateways.Response;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Booking
{
    public class BookingProviderTests
    {
        private readonly BookingProvider _bookigProvider;

        private readonly Mock<IBookingServiceGateway> _mockBookingServiceGateway = new Mock<IBookingServiceGateway>();

        public BookingProviderTests()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse>{ StatusCode = System.Net.HttpStatusCode.OK, IsSuccessStatusCode = true, ResponseContent = new AvailabilityDayResponse() });

            _bookigProvider = new BookingProvider(_mockBookingServiceGateway.Object);
        }

        [Fact]
        public async Task NextAvailability_ShouldReturnDayResponse_OnSuccessfullyCall()
        {
            var request = new AvailabilityRequest();

            var result = await _bookigProvider.NextAvailability(request);

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.IsType<AvailabilityDayResponse>(result);
        }

        [Fact]
        public async Task NextAvailability_Should_Throw_ApplicationException_OnBadRequest()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse>{ StatusCode = System.Net.HttpStatusCode.BadRequest, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway received a bad request, Request:", result.Message);
        }

        [Fact]
        public async Task NextAvailability_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotFound()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse>{ StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<BookingNoAvailabilityException>(() => _bookigProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway returned with 404 status code, no appointments available within the requested timeframe", result.Message);
        }
        
        
        [Fact]
        public async Task NextAvailability_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotSuccessful()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse>{ StatusCode = System.Net.HttpStatusCode.InternalServerError, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway returned with non success status code of InternalServerError, Response: ", result.Message);
        }
        
    }
}
