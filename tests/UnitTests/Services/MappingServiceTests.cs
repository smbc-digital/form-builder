using form_builder.Enum;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using form_builder_tests.Builders;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class MappingServiceTests
    {
        private readonly MappingService _service;
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistrubutedCache = new Mock<IDistributedCacheWrapper>();

        public MappingServiceTests()
        {
            _service = new MappingService(_schemaProvider.Object, _mockDistrubutedCache.Object);
        }

        [Fact(Skip ="wip")]
        public async Task Map_ShouldCallCacheProvider_ToGetFormData()
        {

            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("test-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            await _service.Map("form", "guid");

            // Assert
            _mockDistrubutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }
    }
}
