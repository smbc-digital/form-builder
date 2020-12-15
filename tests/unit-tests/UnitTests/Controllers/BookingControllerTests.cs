using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Controllers;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Services.BookingService;
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

            _bookingController = new BookingController(_bookingService.Object, _schemaFactory.Object, _pageHelper.Object, _sessionHelper.Object);
            _bookingController.ControllerContext = new ControllerContext();
            _bookingController.ControllerContext.HttpContext = new DefaultHttpContext();
            _bookingController.ControllerContext.HttpContext.Request.Query = new QueryCollection();
        }

        [Fact]
        public async void Index_Should_Throw_ApplicationException_InvalidForm()
        {
            // Arrange
            const string form = "invalid";
            const string path = "irrelevant";
            var viewModel = new Dictionary<string, string[]>();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingController.Index(form, path, viewModel));
            Assert.Equal($"Requested form '{form}' could not be found.", result.Message);
            _bookingService.Verify(_ => _.ProcessMonthRequest(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<Page>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void Index_Should_Throw_ApplicationException_InvalidPath()
        {
            // Arrange
            const string form = "valid";
            const string path = "invalid";
            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _bookingController.Index(form, path, viewModel));
            Assert.Equal($"Requested path '{path}' object could not be found for form '{form}'", result.Message);
            _bookingService.Verify(_ => _.ProcessMonthRequest(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<Page>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void Index_Should_RedirectTo_HomeController_Index_WithRouteValues()
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
            _bookingService.Verify(_ => _.ProcessMonthRequest(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<Page>(), It.IsAny<string>()), Times.Once);
        }
    }
}
