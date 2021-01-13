using System;
using System.Collections.Generic;
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
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse> { StatusCode = System.Net.HttpStatusCode.OK, IsSuccessStatusCode = true, ResponseContent = new AvailabilityDayResponse() });

            _mockBookingServiceGateway.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<List<AvailabilityDayResponse>> { StatusCode = System.Net.HttpStatusCode.OK, IsSuccessStatusCode = true, ResponseContent = new List<AvailabilityDayResponse>() });

            _mockBookingServiceGateway.Setup(_ => _.Reserve(It.IsAny<BookingRequest>()))
                .ReturnsAsync(new HttpResponse<Guid> { StatusCode = System.Net.HttpStatusCode.OK, IsSuccessStatusCode = true, ResponseContent = Guid.Empty });

            _mockBookingServiceGateway.Setup(_ => _.GetLocation(It.IsAny<LocationRequest>()))
                .ReturnsAsync(new HttpResponse<string> { StatusCode = System.Net.HttpStatusCode.OK, IsSuccessStatusCode = true, ResponseContent = String.Empty });

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
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse> { StatusCode = System.Net.HttpStatusCode.BadRequest, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway received a bad request, Request:", result.Message);
        }

        [Fact]
        public async Task NextAvailability_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotFound()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse> { StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<BookingNoAvailabilityException>(() => _bookigProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway returned with 404 status code, no appointments available within the requested timeframe", result.Message);
        }


        [Fact]
        public async Task NextAvailability_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotSuccessful()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse> { StatusCode = System.Net.HttpStatusCode.InternalServerError, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway returned with non success status code of InternalServerError, Response: ", result.Message);
        }

        [Fact]
        public async Task GetAvailability_ShouldReturn_ListOfDayResponse_OnSuccessfullyCall()
        {
            var request = new AvailabilityRequest();

            var result = await _bookigProvider.GetAvailability(request);

            _mockBookingServiceGateway.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.IsType<List<AvailabilityDayResponse>>(result);
        }

        [Fact]
        public async Task GetAvailability_Should_Throw_ApplicationException_OnBadRequest()
        {
            _mockBookingServiceGateway.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<List<AvailabilityDayResponse>> { StatusCode = System.Net.HttpStatusCode.BadRequest, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.GetAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::GetAvailability, BookingServiceGateway received a bad request, Request:", result.Message);
        }

        [Fact]
        public async Task GetAvailability_Should_Throw_ApplicationException_WhenBookingType_CannotBeFound()
        {
            _mockBookingServiceGateway.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<List<AvailabilityDayResponse>> { StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.GetAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::GetAvailability, BookingServiceGateway returned 404 status code, booking with id ", result.Message);
        }


        [Fact]
        public async Task GetAvailability_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotSuccessful()
        {
            _mockBookingServiceGateway.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<List<AvailabilityDayResponse>> { StatusCode = System.Net.HttpStatusCode.InternalServerError, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.GetAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::GetAvailability, BookingServiceGateway returned with non success status code of InternalServerError, Response: ", result.Message);
        }

        [Fact]
        public async Task Reserve_ShouldReturn_Guid_OnSuccessfullyCall()
        {
            var request = new BookingRequest();

            var result = await _bookigProvider.Reserve(request);

            _mockBookingServiceGateway.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            Assert.IsType<Guid>(result);
        }

        [Fact]
        public async Task Reserve_Should_Throw_ApplicationException_OnBadRequest()
        {
            _mockBookingServiceGateway.Setup(_ => _.Reserve(It.IsAny<BookingRequest>()))
                .ReturnsAsync(new HttpResponse<Guid> { StatusCode = System.Net.HttpStatusCode.BadRequest, IsSuccessStatusCode = false });

            var request = new BookingRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.Reserve(request));

            _mockBookingServiceGateway.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::Reserve, BookingServiceGateway received a bad request, Request", result.Message);
        }

        [Fact]
        public async Task Reserve_Should_Throw_ApplicationException_WhenBookingType_CannotBeFound()
        {
            _mockBookingServiceGateway.Setup(_ => _.Reserve(It.IsAny<BookingRequest>()))
                .ReturnsAsync(new HttpResponse<Guid> { StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new BookingRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.Reserve(request));

            _mockBookingServiceGateway.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::Reserve, BookingServiceGateway returned 404 status code, booking with id ", result.Message);
        }


        [Fact]
        public async Task Reserve_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotSuccessful()
        {
            _mockBookingServiceGateway.Setup(_ => _.Reserve(It.IsAny<BookingRequest>()))
                .ReturnsAsync(new HttpResponse<Guid> { StatusCode = System.Net.HttpStatusCode.InternalServerError, IsSuccessStatusCode = false });

            var request = new BookingRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.Reserve(request));

            _mockBookingServiceGateway.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::Reserve, BookingServiceGateway returned with non success status code of InternalServerError, Response: ", result.Message);
        }

        [Fact]
        public async Task GetLocation_ShouldReturnLocationResponse_OnSuccessfulCall()
        {
            var request = new LocationRequest();

            var result = await _bookigProvider.GetLocation(request);

            _mockBookingServiceGateway.Verify(_ => _.GetLocation(It.IsAny<LocationRequest>()), Times.Once);
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task GetLocation_Should_Throw_NotFoundExceptionn_WhenAppointmentNotAvailable()
        {
            _mockBookingServiceGateway.Setup(_ => _.GetLocation(It.IsAny<LocationRequest>()))
                .ReturnsAsync(new HttpResponse<string> { StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new LocationRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookigProvider.GetLocation(request));

            _mockBookingServiceGateway.Verify(_ => _.GetLocation(It.IsAny<LocationRequest>()), Times.Once);
            Assert.Contains("cannot be found", result.Message);
        }
    }
}
