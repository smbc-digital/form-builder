using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using form_builder.Configuration;
using form_builder.Gateways;
using form_builder.Providers.Transforms.PaymentConfiguration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.TransformDataProvider
{
    public class S3PaymentConfigurationTransformDataProviderTests
    {
        private readonly S3PaymentConfigurationTransformDataProvider _s3TransformProvider;
        private readonly Mock<IS3Gateway> _mockS3Gateway = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        private readonly Mock<IConfiguration> _mockConfiguration = new();

        public S3PaymentConfigurationTransformDataProviderTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
            _mockConfiguration.Setup(_ => _["S3BucketKey"]).Returns("forms-storage");
            _s3TransformProvider = new S3PaymentConfigurationTransformDataProvider(_mockS3Gateway.Object, _mockHostingEnv.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Get_ShouldCallS3Gateway()
        {
            // Arrange
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AmazonS3Exception("an exception"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => _s3TransformProvider.Get<List<PaymentInformation>>());

            // Assert
            _mockS3Gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsAmazonException()
        {
            // Arrange
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AmazonS3Exception("amazon exception"));

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _s3TransformProvider.Get<List<PaymentInformation>>());
            Assert.StartsWith("S3PaymentConfigurationTransformDataProvider: An error has occured while attempting to get S3 Object, Exception:", result.Message);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsException()
        {
            // Arrange
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("an exception"));

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _s3TransformProvider.Get<List<PaymentInformation>>());
            Assert.StartsWith("S3PaymentConfigurationTransformDataProvider: An error has occured while attempting to deserialise object, Exception:", result.Message);
        }
    }
}
