using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Cache
{
    public class CacheTests
    {
        private readonly form_builder.Cache.Cache _cache;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<ISchemaProvider> _mockSchemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IOptions<DistrbutedCacheConfiguration>> _mockDistrbutedCacheSettings = new Mock<IOptions<DistrbutedCacheConfiguration>>();

        public CacheTests()
        {
            _mockDistrbutedCacheSettings.Setup(_ => _.Value).Returns(new DistrbutedCacheConfiguration
            {
                UseDistrbutedCache = true
            });

            _cache = new form_builder.Cache.Cache(_mockDistributedCacheWrapper.Object, _mockSchemaProvider.Object, _mockDistrbutedCacheSettings.Object);
        }

        [Fact]
        public async Task GetFromCacheOrDirectlyFromSchemaAsync_ShouldCallSchemaProvider_WhenUseDistrbutedCache_IsFalse()
        {
            _mockDistrbutedCacheSettings.Setup(_ => _.Value).Returns(new DistrbutedCacheConfiguration
            {
                UseDistrbutedCache = false
            });

            await _cache.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>("form", 10, ESchemaType.FormJson);

            _mockSchemaProvider.Verify(_ => _.Get<FormSchema>(It.IsAny<string>()), Times.Once);
            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
            _mockDistributedCacheWrapper.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TaskGetFromCacheOrDirectlyFromSchemaAsync_ShouldCallSchemaProvider_WhenMinutesIsZero()
        {
            await _cache.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>("", 0, ESchemaType.FormJson);

            _mockSchemaProvider.Verify(_ => _.Get<FormSchema>(It.IsAny<string>()), Times.Once);
            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
            _mockDistributedCacheWrapper.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task TaskGetFromCacheOrDirectlyFromSchemaAsync_ShouldReturnDataWhenFoundInCache()
        {
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormSchema()));

            var result = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>("testform", 10, ESchemaType.FormJson);

            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.Is<string>(x => x == "form-json-testform")), Times.Once);
            _mockSchemaProvider.Verify(_ => _.Get<FormSchema>(It.IsAny<string>()), Times.Never);
            _mockDistributedCacheWrapper.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.IsType<FormSchema>(result);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TaskGetFromCacheOrDirectlyFromSchemaAsync_ShouldCheckForData_InCache_Then_CallAndSetDataUsingSchema()
        {
            var minutes = 10;

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(new FormSchema());

            var result = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>("testform", minutes, ESchemaType.FormJson);

            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.Is<string>(x => x == "form-json-testform")), Times.Once);
            _mockSchemaProvider.Verify(_ => _.Get<FormSchema>(It.IsAny<string>()), Times.Once);
            _mockDistributedCacheWrapper.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<int>(x => x == minutes), It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsType<FormSchema>(result);
            Assert.NotNull(result);
        }
    }
}
