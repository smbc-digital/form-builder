using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Lookup;
using form_builder.Services.RetrieveExternalDataService.Entities;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class DynamicLookupPageTransformFactoryTests
    {
        private readonly Mock<IActionHelper> _mockActionHelper = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly Mock<IPageHelper> _mockPageHelper = new();
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

            _mockSessionHelper
                .Setup(_ => _.GetSessionGuid())
                .Returns("12345");
            _mockPageHelper
                .Setup(_ => _.GetSavedAnswers(It.IsAny<string>()))
                .Returns(new FormAnswers());
            _mockActionHelper
                .Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                    {
                        IsPost = false,
                        Url = "waste"
                    });
            _mockWebHostEnvironment.Setup(_ => _.EnvironmentName).Returns("local");

            _dynamicLookupPageTransformFactory = new DynamicLookupPageTransformFactory(_mockActionHelper.Object,
                _mockSessionHelper.Object,
                _mockLookupProviders,
                _mockPageHelper.Object,
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
            var result = await Assert.ThrowsAsync<Exception>(() => _dynamicLookupPageTransformFactory.Transform(page, "12345"));
            Assert.Equal("DynamicLookupPageTransformFactory::AddDynamicOptions, No Environment specific details found", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldCall_SessionHelper_PageHelper_And_ActionHelper()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "fake",
                    URL = "waste",
                    AuthToken = "token"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _dynamicLookupPageTransformFactory.Transform(page, "12345");

            // Assert
            _mockSessionHelper.Verify(_ => _.GetSessionGuid(), Times.Once);
            _mockPageHelper.Verify(_ => _.GetSavedAnswers(It.IsAny<string>()), Times.Once);
            _mockActionHelper.Verify(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task Transform_ShouldNotAddAnyOptions_IfUrlIsNull()
        {
            // Arrange
            _mockActionHelper
                .Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                {
                    IsPost = false,
                    Url = null
                });

            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "fake",
                    URL = "waste"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _dynamicLookupPageTransformFactory.Transform(page, "12345");

            // Assert
            Assert.Empty(element.Properties.Options);
        }

        [Fact]
        public async Task Transform_ShouldNotCallLookupProvider_IfUrlIsNull()
        {
            // Arrange
            _mockActionHelper
                .Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                {
                    IsPost = false,
                    Url = null
                });

            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "fake",
                    URL = "waste"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _dynamicLookupPageTransformFactory.Transform(page, "12345");

            // Assert
            _fakeLookupProvider.Verify(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Transform_ShouldNotSaveFormData_IfUrlIsNull()
        {
            // Arrange
            _mockActionHelper
                .Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                {
                    IsPost = false,
                    Url = null
                });

            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "fake",
                    URL = "waste"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _dynamicLookupPageTransformFactory.Transform(page, "12345");

            // Assert
            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
                    URL = "test",
                    AuthToken = "test"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _dynamicLookupPageTransformFactory.Transform(page, "12345"));
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
                    URL = "test",
                    AuthToken = "test",
                    Provider = "not-found"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dynamicLookupPageTransformFactory.Transform(page, "12345"));
        }

        [Fact]
        public async Task Transform_ShouldNotCallLookupProvider_IfOptionsFoundInCachedAnswers()
        {
            // Arrange
            var formAnswers = new FormAnswers
            {
                FormData = new Dictionary<string, object>
                {
                    { "waste", new List<Option> { new Option { Text = "option", Value = "option" } } }
                }
            };

            _mockPageHelper
                .Setup(_ => _.GetSavedAnswers(It.IsAny<string>()))
                .Returns(formAnswers);

            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "fake",
                    URL = "waste"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _dynamicLookupPageTransformFactory.Transform(page, "12345");

            // Assert
            _fakeLookupProvider.Verify(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Transform_ShouldCallLookupProvider_IfOptionsNotFoundInCachedAnswers()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("dynamicQuestion")
                .WithLookup("dynamic")
                .WithLookupSource(new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "fake",
                    URL = "waste",
                    AuthToken = "test"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _dynamicLookupPageTransformFactory.Transform(page, "12345");

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
                    URL = "waste",
                    AuthToken = "test",
                    Provider = "fake"
                })
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _dynamicLookupPageTransformFactory.Transform(page, "12345"));
            Assert.Equal("DynamicLookupPageTransformFactory::AddDynamicOptions, Provider returned no options", result.Message);
        }
    }
}
