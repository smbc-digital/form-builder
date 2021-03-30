using System;
using System.Threading.Tasks;
using Amazon.S3;
using form_builder.Configuration;
using form_builder.Gateways;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.SchemaProvider
{
    public class S3FileSchemaProviderTests
    {
        private readonly S3FileSchemaProvider _s3Schema;
        private readonly Mock<IS3Gateway> _mockS3Gateway = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new();
        private readonly Mock<IOptions<DistributedCacheConfiguration>> _mockDistributedCacheConfiguration = new();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationConfiguration = new();
        private readonly Mock<ILogger<ISchemaProvider>> _mockLogger = new();

        public S3FileSchemaProviderTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("uitest");
            _mockConfiguration.Setup(_ => _["S3BucketKey"]).Returns("forms-storage");
            _s3Schema = new S3FileSchemaProvider(_mockS3Gateway.Object, _mockHostingEnv.Object, _mockDistributedCacheWrapper.Object, _mockConfiguration.Object, _mockDistributedCacheConfiguration.Object, _mockDistributedCacheExpirationConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsAmazonException()
        {
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AmazonS3Exception("amazon exception"));

            var result = await Assert.ThrowsAsync<Exception>(() => _s3Schema.Get<string>("name"));
            _mockS3Gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.StartsWith("S3FileSchemaProvider: An error has occured while attempting to get S3 Object, Exception:", result.Message);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsException()
        {
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("an exception"));

            var result = await Assert.ThrowsAsync<Exception>(() => _s3Schema.Get<string>("name"));
            _mockS3Gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.StartsWith("S3FileSchemaProvider: An error has occured while attempting to deserialize object, Exception:", result.Message);
        }
    }
}
