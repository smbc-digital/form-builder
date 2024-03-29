using Amazon.S3;
using form_builder.Configuration;
using form_builder.Gateways;
using form_builder.Providers.Transforms.ReusableElements;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.TransformDataProvider
{
    public class S3ReusableElementTransformDataProviderTests
    {
        private readonly S3ReusableElementTransformDataProvider _s3TransformProvider;
        private readonly Mock<IS3Gateway> _mockS3Gateway = new();
        private readonly Mock<ILogger<S3ReusableElementTransformDataProvider>> _mockLogger = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        private readonly Mock<IOptions<S3SchemaProviderConfiguration>> _mockConfiguration = new();

        public S3ReusableElementTransformDataProviderTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("uitest");
            _mockConfiguration.Setup(_ => _.Value).Returns(new S3SchemaProviderConfiguration { S3BucketKey = "forms-storage" });
            _s3TransformProvider = new S3ReusableElementTransformDataProvider(_mockS3Gateway.Object, _mockLogger.Object, _mockHostingEnv.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsAmazonException()
        {
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AmazonS3Exception("amazon exception"));

            var result = await Assert.ThrowsAsync<Exception>(() => _s3TransformProvider.Get("name"));
            _mockS3Gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.StartsWith("S3ReusableElementTransformDataProvider: An error has occured while attempting to get S3 Object, Exception:", result.Message);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsException()
        {
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("an exception"));

            var result = await Assert.ThrowsAsync<Exception>(() => _s3TransformProvider.Get("name"));
            _mockS3Gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.StartsWith("S3ReusableElementTransformDataProvider: An error has occured while attempting to deserialise object, Exception:", result.Message);
        }
    }
}
