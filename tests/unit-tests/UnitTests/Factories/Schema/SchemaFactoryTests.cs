using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Factories.Transform.Lookups;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Schema
{
    public class SchemaFactoryTests
    {
        private readonly SchemaFactory _schemaFactory;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new();
        private readonly Mock<ISchemaProvider> _mockSchemaProvider = new();
        private readonly Mock<ILookupSchemaTransformFactory> _mockLookupSchemaFactory = new();
        private readonly Mock<IReusableElementSchemaTransformFactory> _mockReusableElementSchemaFactory = new();
        private readonly Mock<IOptions<DistributedCacheConfiguration>> _mockDistributedCacheConfiguration = new();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationConfiguration = new();
        private readonly Mock<IOptions<PreviewModeConfiguration>> _mockPreviewModeConfiguration = new();
        private readonly Mock<IFormSchemaIntegrityValidator> _mockFormSchemaIntegrityValidator = new();
        private readonly Mock<IEnumerable<IUserPageTransformFactory>> _mockUserPageFactories = new();
        private readonly Mock<IUserPageTransformFactory> _mockUserPageFactory = new();

        public SchemaFactoryTests()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration());

            var mockUserPageFactoryItems = new List<IUserPageTransformFactory> { _mockUserPageFactory.Object };
            _mockUserPageFactories
                .Setup(m => m.GetEnumerator())
                .Returns(() => mockUserPageFactoryItems.GetEnumerator());

            _mockDistributedCacheExpirationConfiguration
                .Setup(_ => _.Value)
                .Returns(new DistributedCacheExpirationConfiguration
                {
                    FormJson = 1
                });

            _mockDistributedCacheConfiguration
                .Setup(_ => _.Value)
                .Returns(new DistributedCacheConfiguration
                {
                    UseDistributedCache = true
                });

            var page = new PageBuilder()
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockLookupSchemaFactory
                .Setup(_ => _.Transform(It.IsAny<FormSchema>()))
                .Returns(formSchema);

            _mockReusableElementSchemaFactory
                .Setup(_ => _.Transform(It.IsAny<FormSchema>()))
                .ReturnsAsync(formSchema);

            _mockSchemaProvider
                .Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            _mockSchemaProvider
                .Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            _mockFormSchemaIntegrityValidator
                .Setup(_ => _.Validate(It.IsAny<FormSchema>()));

            _schemaFactory = new SchemaFactory(
                _mockDistributedCache.Object,
                _mockSchemaProvider.Object,
                _mockLookupSchemaFactory.Object,
                _mockReusableElementSchemaFactory.Object,
                _mockDistributedCacheConfiguration.Object,
                _mockDistributedCacheExpirationConfiguration.Object,
                _mockPreviewModeConfiguration.Object,
                _mockFormSchemaIntegrityValidator.Object,
                _mockUserPageFactories.Object);
        }

        [Fact]
        public async Task Build_ShouldCallDistributedCache_When_CacheEnabled()
        {
            // Arrange
            _mockSchemaProvider
                .Setup(_ => _.ValidateSchemaName(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            await _schemaFactory.Build("form");

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Build_ShouldReturnFormJson_WhenFoundInCache()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .Build();

            _mockSchemaProvider
                .Setup(_ => _.ValidateSchemaName(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockDistributedCache
                .Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(formSchema));

            // Act
            var result = await _schemaFactory.Build("form");

            // Assert
            Assert.IsType<FormSchema>(result);
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockLookupSchemaFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Never);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockFormSchemaIntegrityValidator.Verify(_ => _.Validate(It.IsAny<FormSchema>()), Times.Never);
        }

        [Fact]
        public async Task Build_ShouldCallLookupFactory_WhenElementContains_Lookup()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithLookup("list-test")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithLookup("lookup-two")
                .Build();

            var element4 = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithLookup("list-test")
                .Build();

            var element5 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element3)
                .WithElement(element4)
                .WithElement(element5)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .Build();

            _mockSchemaProvider
                .Setup(_ => _.ValidateSchemaName(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockSchemaProvider
                .Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            _mockReusableElementSchemaFactory
                .Setup(_ => _.Transform(It.IsAny<FormSchema>()))
                .ReturnsAsync(formSchema);

            // Act
            await _schemaFactory.Build("form");

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockLookupSchemaFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Once);
            _mockReusableElementSchemaFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Once);
        }

        [Fact]
        public async Task Build_Should_SaveFormSchema_()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Select)
                .Build();

            var element4 = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .Build();

            var element5 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element3)
                .WithElement(element4)
                .WithElement(element5)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .Build();

            _mockSchemaProvider
                .Setup(_ => _.ValidateSchemaName(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockSchemaProvider
                .Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            _mockLookupSchemaFactory
                .Setup(_ => _.Transform(It.IsAny<FormSchema>()))
                .Returns(formSchema);

            _mockReusableElementSchemaFactory
                .Setup(_ => _.Transform(It.IsAny<FormSchema>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = await _schemaFactory.Build("form");

            // Assert
            Assert.IsType<FormSchema>(result);
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockFormSchemaIntegrityValidator.Verify(_ => _.Validate(It.IsAny<FormSchema>()), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAbsoluteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TransformPage_ShouldCallEachTransformFactory()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _schemaFactory.TransformPage(page, new FormAnswers());

            // Assert
            _mockUserPageFactory.Verify(_ => _.Transform(It.IsAny<Page>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task Build_Should_Throw_Exception_WhenPreview_FormSchema_HasExpired()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration { IsEnabled = true });

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _schemaFactory.Build(PreviewConstants.PREVIEW_MODE_PREFIX));

            _mockDistributedCache.Verify(_ => _.GetString($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}"));
            _mockFormSchemaIntegrityValidator.Verify(_ => _.Validate(It.IsAny<FormSchema>()), Times.Never);
            _mockSchemaProvider.Verify(_ => _.ValidateSchemaName(It.IsAny<string>()), Times.Never);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Build_Should_CallDistrbutedCache_AndReturn_Preview_FormSchema_WhenPreviewMode_Enabled_AndFormName_Matches()
        {
            _mockDistributedCache
            .Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new FormSchema { Pages = new List<Page>(), BaseURL = PreviewConstants.PREVIEW_MODE_PREFIX }));

            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration { IsEnabled = true });

            var result = await _schemaFactory.Build(PreviewConstants.PREVIEW_MODE_PREFIX);

            _mockDistributedCache.Verify(_ => _.GetString($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}"));
            _mockFormSchemaIntegrityValidator.Verify(_ => _.Validate(It.IsAny<FormSchema>()), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockSchemaProvider.Verify(_ => _.ValidateSchemaName(It.IsAny<string>()), Times.Never);
            var formSchemaResult = Assert.IsType<FormSchema>(result);
        }
    }
}