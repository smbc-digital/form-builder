using Amazon.S3;
using form_builder.Gateways;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.Transforms.Lookups;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.TransformDataProvider
{
    public class S3TransformDataProviderTests
    {
        private readonly S3LookupTransformDataProvider _s3TransformProvider;
        private readonly Mock<IS3Gateway> _mockS3gateway = new Mock<IS3Gateway>();
        private readonly Mock<ILogger<S3LookupTransformDataProvider>> _mockLogger = new Mock<ILogger<S3LookupTransformDataProvider>>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();

        private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();

        public S3TransformDataProviderTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("uitest");
            _mockConfiguration.Setup(_ => _["S3BucketKey"]).Returns("forms-storage");
            _s3TransformProvider = new S3LookupTransformDataProvider(_mockS3gateway.Object, _mockLogger.Object, _mockHostingEnv.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsAmazonException()
        {
            _mockS3gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AmazonS3Exception("amazon exception"));

            var result = await Assert.ThrowsAsync<Exception>(() => _s3TransformProvider.Get<string>("name"));
            _mockS3gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.StartsWith("S3TransformDataProvider: An error has occured while attempting to get S3 Object, Exception:", result.Message);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsException()
        {
            _mockS3gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("an exception"));

            var result = await Assert.ThrowsAsync<Exception>(() => _s3TransformProvider.Get<string>("name"));
            _mockS3gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.StartsWith("S3TransformDataProvider: An error has occured while attempting to deserialise object, Exception:", result.Message);
        }
    }
}
