using Amazon.S3;
using Amazon.S3.Model;
using form_builder.Gateways;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Gateway
{
    public class S3GatewayTests
    {
        private readonly Mock<IAmazonS3> _mockS3Client = new();
        private readonly S3Gateway _s3Gateway;

        public S3GatewayTests()
        {
            _s3Gateway = new S3Gateway(_mockS3Client.Object);
        }

        [Fact]
        public async Task GetObject_ShouldCallS3Client()
        {
            // Arrange
            _mockS3Client
                .Setup(_ => _.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse());

            // Act
            await _s3Gateway.GetObject("bucketName", "key");

            // Assert
            _mockS3Client.Verify(_ => _.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}