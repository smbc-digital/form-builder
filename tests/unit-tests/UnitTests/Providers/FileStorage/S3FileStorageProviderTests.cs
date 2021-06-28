using System.IO;
using System.Threading.Tasks;
using form_builder.Gateways;
using form_builder.Providers.FileStorage;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.FileStorage
{
    public class S3FileStorageProviderTests
    {
        private readonly S3FileStorageProvider _s3FileStorageProvider;
        private readonly Mock<IS3Gateway> _mockS3Gateway = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();

        public S3FileStorageProviderTests()
        {
            _mockConfiguration.Setup(_ => _["FileStorageProvider:Type"]).Returns("S3");
            _mockConfiguration.Setup(_ => _["FileStorageProvider:S3BucketName"]).Returns("formbuilder-s3-file-storage");
            _s3FileStorageProvider = new S3FileStorageProvider(_mockS3Gateway.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetString_ShouldCallS3GetObject()
        {
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Amazon.S3.Model.GetObjectResponse()
                {
                    BucketName = "TestBucket",
                    Key = "Test",
                    ResponseStream = new MemoryStream()
                });

            await _s3FileStorageProvider.GetString(It.IsAny<string>());

            _mockS3Gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Remove_ShouldCallS3DeleteObject()
        {
            await _s3FileStorageProvider.Remove(It.IsAny<string>());

            _mockS3Gateway.Verify(_ => _.DeleteObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SetStringAsync_ShouldCallDS3PutObject()
        {
            await _s3FileStorageProvider.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());

            _mockS3Gateway.Verify(_ => _.PutObject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
