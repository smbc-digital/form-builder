using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Models;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Lookup;
using form_builder.Services.RetrieveExternalDataService.Entities;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class BookingLookupPageTransformFactoryTests
    {
        private readonly Mock<IActionHelper> _mockActionHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment = new();
        private readonly IEnumerable<ILookupProvider> _mockLookupProviders;
        private readonly Mock<ILookupProvider> _fakeLookupProvider = new();
        private readonly BookingLookupPageTransformFactory _bookingLookupPageTransformFactory;

        public BookingLookupPageTransformFactoryTests()
        {
            _fakeLookupProvider.Setup(_ => _.ProviderName).Returns("fake");

            _fakeLookupProvider.Setup(_ => _.GetAppointmentTypesAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<AppointmentType>
                {
                    new()
                });

            _mockLookupProviders = new List<ILookupProvider>
            {
                _fakeLookupProvider.Object
            };

            _mockActionHelper
                .Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                {
                    IsPost = false,
                    Url = "trip=trip"
                });

            _mockWebHostEnvironment
                .Setup(_ => _.EnvironmentName)
                .Returns("local");

            _bookingLookupPageTransformFactory = new BookingLookupPageTransformFactory(_mockActionHelper.Object,
                _mockLookupProviders, 
                _mockWebHostEnvironment.Object);
        }

        [Fact]
        public async Task Transform_ShouldThrowIfSubmitDetailsNull()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "test"
                })
                .WithType(EElementType.Booking)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _bookingLookupPageTransformFactory.Transform(page, new FormAnswers()));
            Assert.Equal($"{nameof(BookingLookupPageTransformFactory)}::AddDynamicAppointmentTypes: No LookUpSource found for environment", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldCall_ActionHelper()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "fake",
                    URL = "trip={{trip}}",
                    AuthToken = "token"
                })
                .WithType(EElementType.Booking)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _bookingLookupPageTransformFactory.Transform(page, new FormAnswers());

            // Assert
            _mockActionHelper.Verify(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task Transform_ShouldThrowIfProviderNullOrEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    URL = "trip={{trip}}",
                    AuthToken = "test"
                })
                .WithType(EElementType.Booking)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _bookingLookupPageTransformFactory.Transform(page, new FormAnswers()));
            Assert.Equal($"{nameof(BookingLookupPageTransformFactory)}::AddDynamicAppointmentTypes: No Provider name given in LookupSources", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldThrowIfProviderNotFound()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    URL = "trip={{trip}}",
                    AuthToken = "test",
                    Provider = "not-found"
                })
                .WithType(EElementType.Booking)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _bookingLookupPageTransformFactory.Transform(page, new FormAnswers()));
        }

        [Fact]
        public async Task Transform_ShouldCallLookupProvider()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "fake",
                    URL = "trip={{trip}}",
                    AuthToken = "test"
                })
                .WithType(EElementType.Booking)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _bookingLookupPageTransformFactory.Transform(page, new FormAnswers());

            // Assert
            _fakeLookupProvider.Verify(_ => _.GetAppointmentTypesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Transform_ShouldThrowIfProviderReturnsNoAppointmentTypes()
        {
            // Arrange
            _fakeLookupProvider.Setup(_ => _.GetAppointmentTypesAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<AppointmentType>());

            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    URL = "trip={{trip}}",
                    AuthToken = "test",
                    Provider = "fake"
                })
                .WithType(EElementType.Booking)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _bookingLookupPageTransformFactory.Transform(page, new FormAnswers()));
            Assert.Equal($"{nameof(BookingLookupPageTransformFactory)}::AddDynamicAppointmentTypes: Provider returned no AppointmentTypes", result.Message);
        }
    }
}
