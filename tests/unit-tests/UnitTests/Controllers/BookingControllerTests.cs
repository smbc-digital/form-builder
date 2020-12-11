using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Controllers;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
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
            const string formName = "invalid";
            const string path = "irrelevant";
            var viewModel = new Dictionary<string, string[]>();

            try
            {
                // Act
                var result = await _bookingController.Index(formName, path, viewModel);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsType<ApplicationException>(ex);
            }
        }

        [Fact]
        public async void Index_Should_Throw_ApplicationException_InvalidPath()
        {
            // Arrange
            const string formName = "valid";
            const string path = "invalid";
            var viewModel = new Dictionary<string, string[]>();

            try
            {
                // Act
                var result = await _bookingController.Index(formName, path, viewModel);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsType<ApplicationException>(ex);
            }
        }

        [Fact]
        public async void Index_Should_RedirectTo_HomeController_Index()
        {
            // Arrange
            const string formName = "valid";
            const string path = "valid";
            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _bookingController.Index(formName, path, viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
