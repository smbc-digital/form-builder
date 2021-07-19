using form_builder.Models;
using form_builder.Providers.FileStorage;
using form_builder.Providers.StorageProvider;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.FileStorage
{
    public class InMemoryFileStorageProviderTests
    {
        private readonly InMemoryStorageProvider _inMemoryStorageProvider;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new();

        public InMemoryFileStorageProviderTests()
        {
            var formData = JsonConvert.SerializeObject(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(formData);

            _inMemoryStorageProvider = new InMemoryStorageProvider(_mockDistributedCache.Object);
        }

        [Fact]
        public async Task GetString_ShouldCallDistributedCacheGetString()
        {
            await _inMemoryStorageProvider.GetString(It.IsAny<string>());

            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Remove_ShouldCallDistributedCacheRemove()
        {
            await _inMemoryStorageProvider.Remove(It.IsAny<string>());

            _mockDistributedCache.Verify(_ => _.RemoveAsync(It.IsAny<string>(), new CancellationToken()), Times.Once);
        }

        [Fact]
        public async Task SetStringAsync_ShouldCallDistributedCacheSetStringAsync()
        {
            await _inMemoryStorageProvider.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());

            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
