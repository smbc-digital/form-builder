using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Factories.Transform.Lookups;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Transforms.Lookups;
using form_builder.Providers.Transforms.ReusableElements;
using form_builder_tests.Builders;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Schema
{
    public class SchemaFactoryTests
    {
        private readonly SchemaFactory _schemaFactory;
        private readonly Mock<IDistributedCacheWrapper> _mockDistrbutedCache =new Mock<IDistributedCacheWrapper>();
        private readonly Mock<ISchemaProvider> _mockSchemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<ILookupSchemaTransformFactory> _mockLookupSchemaFactory  = new Mock<ILookupSchemaTransformFactory>();
        private readonly Mock<IReusableElementTransformDataProvider> _mockReusableElementSchemaProvider = new Mock<IReusableElementTransformDataProvider>();
        private readonly Mock<IReusableElementSchemaTransformFactory> _mockReusableElementSchemaFactory  = new Mock<IReusableElementSchemaTransformFactory>();
        private readonly Mock<IOptions<DistrbutedCacheConfiguration>> _mockDistrbutedCacheConfiguration = new Mock<IOptions<DistrbutedCacheConfiguration>>();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistrbutedCacheExpirationConfiguration = new Mock<IOptions<DistributedCacheExpirationConfiguration>>();

        public SchemaFactoryTests()
        {
            _mockDistrbutedCacheExpirationConfiguration.Setup(_ => _.Value).Returns(new DistributedCacheExpirationConfiguration
            {
                FormJson = 1
            });

            _mockDistrbutedCacheConfiguration.Setup(_ => _.Value).Returns(new DistrbutedCacheConfiguration
            {
                UseDistrbutedCache = true
            });

            var formSchema = new FormSchemaBuilder()
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            _schemaFactory = new SchemaFactory(_mockDistrbutedCache.Object, _mockSchemaProvider.Object, _mockLookupSchemaFactory.Object, _mockReusableElementSchemaFactory.Object, _mockDistrbutedCacheConfiguration.Object,_mockDistrbutedCacheExpirationConfiguration.Object);
        }

        [Fact]
        public async Task Build_ShouldCallDistributedCache_When_CacheEnabled()
        {
            var result = await _schemaFactory.Build("form");

            _mockDistrbutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task Build_ShouldReturnFormJson_WhenFoundInCache()
        {
            var formSchema = new FormSchemaBuilder()
                .Build();

            _mockDistrbutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(formSchema));

            var result = await _schemaFactory.Build("form");

            Assert.IsType<FormSchema>(result);
            _mockDistrbutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockLookupSchemaFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Never);
            _mockDistrbutedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Build_ShouldCallLookupFactory_WhenElementContains_Lookup()
        {
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

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            var result = await _schemaFactory.Build("form");

            Assert.IsType<FormSchema>(result);
            _mockDistrbutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockLookupSchemaFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Exactly(3));
        }

        [Fact]
        public async Task Build_ShouldNotCallLookupFactory_WhenElement_DoesNot_Contain_Lookup()
        {
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

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            var result = await _schemaFactory.Build("form");

            Assert.IsType<FormSchema>(result);
            _mockDistrbutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockLookupSchemaFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Never);
        }

        [Fact]
        public async Task Build_Should_SaveFormScheam_()
        {
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

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            var result = await _schemaFactory.Build("form");

            Assert.IsType<FormSchema>(result);
            _mockDistrbutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockDistrbutedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
