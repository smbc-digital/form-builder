using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Factories.Transform.Lookups;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Schema
{
    public class SchemaFactoryTests
    {
        private readonly SchemaFactory _schemaFactory;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<ISchemaProvider> _mockSchemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<ILookupSchemaTransformFactory> _mockLookupSchemaFactory = new Mock<ILookupSchemaTransformFactory>();
        private readonly Mock<IReusableElementSchemaTransformFactory> _mockReusableElementSchemaFactory = new Mock<IReusableElementSchemaTransformFactory>();
        private readonly Mock<IOptions<DistributedCacheConfiguration>> _mockDistributedCacheConfiguration = new Mock<IOptions<DistributedCacheConfiguration>>();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationConfiguration = new Mock<IOptions<DistributedCacheExpirationConfiguration>>();
        private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private readonly Mock<IFormSchemaIntegrityValidator> _mockFormSchemaIntegrityValidator = new Mock<IFormSchemaIntegrityValidator>();

        public SchemaFactoryTests()
        {
            _mockConfiguration
                .Setup(_ => _["ApplicationVersion"])
                .Returns("v2");

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

            var formSchema = new FormSchemaBuilder()
                .Build();

            _mockSchemaProvider
                .Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            _mockSchemaProvider
                .Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            _mockFormSchemaIntegrityValidator
                .Setup(_ => _.Validate(It.IsAny<FormSchema>()));

            _schemaFactory = new SchemaFactory(_mockDistributedCache.Object, _mockSchemaProvider.Object, _mockLookupSchemaFactory.Object, _mockReusableElementSchemaFactory.Object, _mockDistributedCacheConfiguration.Object, _mockDistributedCacheExpirationConfiguration.Object, _mockConfiguration.Object, _mockFormSchemaIntegrityValidator.Object);
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
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}