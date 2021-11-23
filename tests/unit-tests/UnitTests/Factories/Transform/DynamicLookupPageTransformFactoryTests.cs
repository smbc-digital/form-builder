using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class DynamicLookupPageTransformFactoryTests
    {
        private readonly Mock<IActionHelper> _mockActionHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment = new();
        private readonly IEnumerable<ILookupProvider> _mockLookupProviders;
        private readonly Mock<ILookupProvider> _fakeLookupProvider = new();
        private readonly DynamicLookupPageTransformFactory _dynamicLookupPageTransformFactory;

        public DynamicLookupPageTransformFactoryTests()
        {
            _fakeLookupProvider.Setup(_ => _.ProviderName).Returns("fake");
            
            _fakeLookupProvider.Setup(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Option> {new Option()});

            _mockLookupProviders = new List<ILookupProvider>
            {
                _fakeLookupProvider.Object
            };

            _mockActionHelper
                .Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                    {
                        IsPost = false,
                        Url = "waste=waste"
                    });
            _mockWebHostEnvironment.Setup(_ => _.EnvironmentName).Returns("local");

            _dynamicLookupPageTransformFactory = new DynamicLookupPageTransformFactory(_mockActionHelper.Object,
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
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _dynamicLookupPageTransformFactory.Transform(page, new FormAnswers()));
            Assert.Equal("DynamicLookupPageTransformFactory::AddDynamicOptions, No Environment specific details found", result.Message);
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
                    URL = "waste={{wasteId}}",
                    AuthToken = "token"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _dynamicLookupPageTransformFactory.Transform(page, new FormAnswers());

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
                    URL = "waste={{wasteId}}",
                    AuthToken = "test"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _dynamicLookupPageTransformFactory.Transform(page, new FormAnswers()));
            Assert.Equal("DynamicLookupPageTransformFactory::AddDynamicOptions, No Provider name given in LookupSources", result.Message);
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
                    URL = "waste={{wasteId}}",
                    AuthToken = "test",
                    Provider = "not-found"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dynamicLookupPageTransformFactory.Transform(page, new FormAnswers()));
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
                    URL = "waste={{wasteId}}",
                    AuthToken = "test"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _dynamicLookupPageTransformFactory.Transform(page, new FormAnswers());

            // Assert
            _fakeLookupProvider.Verify(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Transform_ShouldThrowIfProviderReturnsNoOptions()
        {
            // Arrange
            _fakeLookupProvider.Setup(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Option>());

            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    URL = "waste={{wasteId}}",
                    AuthToken = "test",
                    Provider = "fake"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _dynamicLookupPageTransformFactory.Transform(page, new FormAnswers()));
            Assert.Equal("DynamicLookupPageTransformFactory::AddDynamicOptions, Provider returned no options", result.Message);
        }
    }
}
