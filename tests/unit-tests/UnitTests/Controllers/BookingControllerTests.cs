using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Controllers;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Services.BookingService;
using form_builder.Services.BookingService.Entities;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Controllers
{
    public class BookingControllerTests
    {
        private readonly BookingController _bookingController;

        private readonly Mock<IBookingService> _bookingService = new Mock<IBookingService>();
        private readonly Mock<IPageService> _pageService = new Mock<IPageService>();
        private readonly Mock<ISchemaFactory> _schemaFactory = new Mock<ISchemaFactory>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();

        public BookingControllerTests()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Booking)
               .WithQuestionId("valid")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("valid")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaFactory.Setup(_ => _.Build("valid"))
                .ReturnsAsync(schema);

            _bookingController = new BookingController(_bookingService.Object, _schemaFactory.Object, _pageService.Object);
            _bookingController.ControllerContext = new ControllerContext();
            _bookingController.ControllerContext.HttpContext = new DefaultHttpContext();
            _bookingController.ControllerContext.HttpContext.Request.Query = new QueryCollection();
        }


        [Fact]
        public async Task Index_Should_RedirectTo_HomeController_Index_WithRouteValues()
        {
            // Arrange
            const string form = "valid";
            const string path = "valid";
            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _bookingController.Index(form, path, viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.RouteValues.TryGetValue(nameof(form), out var formName);
            redirectResult.RouteValues.TryGetValue(nameof(path), out var pathName);

            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("Index", redirectResult.ActionName);

            Assert.Equal(form, formName);
            Assert.Equal(path, pathName);
            _bookingService.Verify(_ => _.ProcessMonthRequest(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CancelBooking_Should_Throw_ApplicatioException_When_Id_IsEmpty()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingController.CancelBooking("hash", Guid.Empty, "path"));
            Assert.StartsWith("BookingController::CancelBooking, Invalid parameters recieved. Id:", result.Message);
        }

        [Fact]
        public async Task CancelBooking_Should_Throw_ApplicatioException_When_Hash_IsEmpty()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingController.CancelBooking(string.Empty, Guid.NewGuid(), "path"));
            Assert.StartsWith("BookingController::CancelBooking, Invalid parameters recieved. Id:", result.Message);
        }


        [Fact]
        public async Task CancelBooking_Should_RedirectToAction_When_Booking_Cannot_Be_Cancelled()
        {
            _bookingService.Setup(_ => _.ValidateCancellationRequest(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .ThrowsAsync(new BookingCannotBeCancelledException(("cancellation not available")));

            var result = await _bookingController.CancelBooking("hash", Guid.NewGuid(), "path");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CannotCancel", redirectResult.ActionName);
        }

        [Fact]
        public async Task CancelBooking_Should_Reutn_View_WhenAble_To_Cancel_Booking()
        {
            _bookingService.Setup(_ => _.ValidateCancellationRequest(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(new CancelledAppointmentInformation());

            var result = await _bookingController.CancelBooking("hash", Guid.NewGuid(), "path");

            var redirectResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("AppointmentDetails", redirectResult.ViewName);
            _bookingService.Verify(_ => _.ValidateCancellationRequest(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CancelBookingPost_Should_Throw_ApplicatioException_When_Id_IsEmpty()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingController.CancelBookingPost("hash", Guid.Empty, "path"));
            Assert.StartsWith("BookingController::CancelBookingPost, Invalid parameters recieved. Id:", result.Message);
        }

        [Fact]
        public async Task CancelBookingPost_Should_Throw_ApplicatioException_When_Hash_IsEmpty()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingController.CancelBookingPost(string.Empty, Guid.NewGuid(), "path"));
            Assert.StartsWith("BookingController::CancelBookingPost, Invalid parameters recieved. Id:", result.Message);
        }

        [Fact]
        public async Task CancelBookingPost_Should_Reutn_Success_View_WhenBooking_Cancelled_Successfully()
        {
            var result = await _bookingController.CancelBookingPost("hash", Guid.NewGuid(), "path");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CancelSuccess", redirectResult.ActionName);
            _bookingService.Verify(_ => _.Cancel(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CannotCancel_ShouldReturn_Cannot_Cancel_View()
        {
            _schemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(new FormSchema());

            var result = await _bookingController.CannotCancel("form");

            var redirectResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("CannotCancel", redirectResult.ViewName);
            _schemaFactory.Verify(_ => _.Build(It.Is<string>(_ => _.Equals("form"))), Times.Once);
        }

        [Fact]
        public async Task CancelSuccess_ShouldReturn_Cannot_Cancel_View()
        {
            _pageService.Setup(_ => _.GetCancelBookingSuccessPage(It.IsAny<string>()))
                .ReturnsAsync(new SuccessPageEntity());

            var result = await _bookingController.CancelSuccess("form");

            var redirectResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("../Home/Success", redirectResult.ViewName);
            _pageService.Verify(_ => _.GetCancelBookingSuccessPage(It.Is<string>(_ => _.Equals("form"))), Times.Once);
        }
    }
}
