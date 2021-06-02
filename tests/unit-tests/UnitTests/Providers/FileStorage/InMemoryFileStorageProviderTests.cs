using form_builder.Models;
using form_builder.Providers.FileStorage;
using form_builder.Providers.StorageProvider;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
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
        public void GetString_ShouldCallDistributedCacheGetString()
        {
            _inMemoryStorageProvider.GetString(It.IsAny<string>());

            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Remove_ShouldCallDistributedCacheRemove()
        {
            _inMemoryStorageProvider.Remove(It.IsAny<string>());

            _mockDistributedCache.Verify(_ => _.Remove(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SetStringAsync_ShouldCallDistributedCacheSetStringAsync()
        {
            _inMemoryStorageProvider.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());

            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
