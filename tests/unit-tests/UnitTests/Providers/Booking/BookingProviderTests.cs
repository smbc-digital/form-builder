using form_builder.Exceptions;
using form_builder.Providers.Booking;
using Moq;
using StockportGovUK.NetStandard.Gateways.BookingService;
using StockportGovUK.NetStandard.Gateways.Response;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Booking
{
    public class BookingProviderTests
    {
        private readonly BookingProvider _bookingProvider;

        private readonly Mock<IBookingServiceGateway> _mockBookingServiceGateway = new();

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

            _mockBookingServiceGateway.Setup(_ => _.Cancel(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });

            _mockBookingServiceGateway.Setup(_ => _.Confirmation(It.IsAny<ConfirmationRequest>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NoContent });

            _bookingProvider = new BookingProvider(_mockBookingServiceGateway.Object);
        }

        [Fact]
        public async Task NextAvailability_ShouldReturnDayResponse_OnSuccessfullyCall()
        {
            var request = new AvailabilityRequest();

            var result = await _bookingProvider.NextAvailability(request);

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.IsType<AvailabilityDayResponse>(result);
        }

        [Fact]
        public async Task NextAvailability_Should_Throw_ApplicationException_OnBadRequest()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse> { StatusCode = System.Net.HttpStatusCode.BadRequest, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway received a bad request, Request:", result.Message);
        }

        [Fact]
        public async Task NextAvailability_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotFound()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse> { StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<BookingNoAvailabilityException>(() => _bookingProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway returned with 404 status code, no appointments available within the requested timeframe", result.Message);
        }


        [Fact]
        public async Task NextAvailability_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotSuccessful()
        {
            _mockBookingServiceGateway.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<AvailabilityDayResponse> { StatusCode = System.Net.HttpStatusCode.InternalServerError, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.NextAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::NextAvailability, BookingServiceGateway returned with non success status code of InternalServerError, Response: ", result.Message);
        }

        [Fact]
        public async Task GetAvailability_ShouldReturn_ListOfDayResponse_OnSuccessfullyCall()
        {
            var request = new AvailabilityRequest();

            var result = await _bookingProvider.GetAvailability(request);

            _mockBookingServiceGateway.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.IsType<List<AvailabilityDayResponse>>(result);
        }

        [Fact]
        public async Task GetAvailability_Should_Throw_ApplicationException_OnBadRequest()
        {
            _mockBookingServiceGateway.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<List<AvailabilityDayResponse>> { StatusCode = System.Net.HttpStatusCode.BadRequest, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.GetAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::GetAvailability, BookingServiceGateway received a bad request, Request:", result.Message);
        }

        [Fact]
        public async Task GetAvailability_Should_Throw_ApplicationException_WhenBookingType_CannotBeFound()
        {
            _mockBookingServiceGateway.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<List<AvailabilityDayResponse>> { StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.GetAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::GetAvailability, BookingServiceGateway returned 404 status code, booking with id ", result.Message);
        }


        [Fact]
        public async Task GetAvailability_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotSuccessful()
        {
            _mockBookingServiceGateway.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new HttpResponse<List<AvailabilityDayResponse>> { StatusCode = System.Net.HttpStatusCode.InternalServerError, IsSuccessStatusCode = false });

            var request = new AvailabilityRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.GetAvailability(request));

            _mockBookingServiceGateway.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::GetAvailability, BookingServiceGateway returned with non success status code of InternalServerError, Response: ", result.Message);
        }

        [Fact]
        public async Task Reserve_ShouldReturn_Guid_OnSuccessfullyCall()
        {
            var request = new BookingRequest();

            var result = await _bookingProvider.Reserve(request);

            _mockBookingServiceGateway.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            Assert.IsType<Guid>(result);
        }

        [Fact]
        public async Task Reserve_Should_Throw_ApplicationException_OnBadRequest()
        {
            _mockBookingServiceGateway.Setup(_ => _.Reserve(It.IsAny<BookingRequest>()))
                .ReturnsAsync(new HttpResponse<Guid> { StatusCode = System.Net.HttpStatusCode.BadRequest, IsSuccessStatusCode = false });

            var request = new BookingRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.Reserve(request));

            _mockBookingServiceGateway.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::Reserve, BookingServiceGateway received a bad request, Request", result.Message);
        }

        [Fact]
        public async Task Reserve_Should_Throw_ApplicationException_WhenBookingType_CannotBeFound()
        {
            _mockBookingServiceGateway.Setup(_ => _.Reserve(It.IsAny<BookingRequest>()))
                .ReturnsAsync(new HttpResponse<Guid> { StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new BookingRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.Reserve(request));

            _mockBookingServiceGateway.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::Reserve, BookingServiceGateway returned 404 status code, booking with id ", result.Message);
        }


        [Fact]
        public async Task Reserve_Should_Throw_BookingNoAvailabilityException_WhenResponse_IsNotSuccessful()
        {
            _mockBookingServiceGateway.Setup(_ => _.Reserve(It.IsAny<BookingRequest>()))
                .ReturnsAsync(new HttpResponse<Guid> { StatusCode = System.Net.HttpStatusCode.InternalServerError, IsSuccessStatusCode = false });

            var request = new BookingRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.Reserve(request));

            _mockBookingServiceGateway.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            Assert.StartsWith("BookingProvider::Reserve, BookingServiceGateway returned with non success status code of InternalServerError, Response: ", result.Message);
        }

        [Fact]
        public async Task GetLocation_ShouldReturnLocationResponse_OnSuccessfulCall()
        {
            var request = new LocationRequest();

            var result = await _bookingProvider.GetLocation(request);

            _mockBookingServiceGateway.Verify(_ => _.GetLocation(It.IsAny<LocationRequest>()), Times.Once);
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task GetLocation_Should_Throw_NotFoundException_WhenAppointmentNotAvailable()
        {
            _mockBookingServiceGateway.Setup(_ => _.GetLocation(It.IsAny<LocationRequest>()))
                .ReturnsAsync(new HttpResponse<string> { StatusCode = System.Net.HttpStatusCode.NotFound, IsSuccessStatusCode = false });

            var request = new LocationRequest();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.GetLocation(request));

            _mockBookingServiceGateway.Verify(_ => _.GetLocation(It.IsAny<LocationRequest>()), Times.Once);
            Assert.Equal($"BookingProvider::GetLocation, BookingServiceGateway returned not found for appointmentId: {request.AppointmentId}", result.Message);
        }

        [Fact]
        public async Task Cancel_Should_Throw_Exception_When_Response_Is_BadRequest()
        {
            var guid = Guid.NewGuid();
            _mockBookingServiceGateway.Setup(_ => _.Cancel(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest });

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.Cancel(guid));

            _mockBookingServiceGateway.Verify(_ => _.Cancel(It.IsAny<string>()), Times.Once);
            Assert.StartsWith($"BookingProvider::Cancel, BookingServiceGateway returned 400 status code, Gateway received bad request, Request:{guid}, Response:", result.Message);
        }

        [Fact]
        public async Task Cancel_Should_Throw_Exception_When_Response_Is_InternalServerError()
        {
            var guid = Guid.NewGuid();
            _mockBookingServiceGateway.Setup(_ => _.Cancel(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.InternalServerError });

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.Cancel(guid));

            _mockBookingServiceGateway.Verify(_ => _.Cancel(It.IsAny<string>()), Times.Once);
            Assert.StartsWith($"BookingProvider::Cancel, BookingServiceGateway returned with non success status code of InternalServerError", result.Message);
        }

        [Fact]
        public async Task Cancel_Should_Throw_Exception_When_Booking_NotFound()
        {
            var guid = Guid.NewGuid();
            _mockBookingServiceGateway.Setup(_ => _.Cancel(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NotFound });

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingProvider.Cancel(guid));

            _mockBookingServiceGateway.Verify(_ => _.Cancel(It.IsAny<string>()), Times.Once);
            Assert.Equal($"BookingProvider::Cancel, BookingServiceGateway return 404 not found for bookingId {guid}", result.Message);
        }

        [Fact]
        public async Task Cancel_Should_Resolve_WhenResponse_Is_Ok()
        {
            await _bookingProvider.Cancel(Guid.NewGuid());

            _mockBookingServiceGateway.Verify(_ => _.Cancel(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Confirm_Should_Resolve_WhenResponse_Is_NoContent()
        {
            var request = new ConfirmationRequest
            {
                BookingId = Guid.NewGuid()
            };

            await _bookingProvider.Confirm(request);

            _mockBookingServiceGateway.Verify(_ => _.Confirmation(It.IsAny<ConfirmationRequest>()), Times.Once);
        }
    }
}
