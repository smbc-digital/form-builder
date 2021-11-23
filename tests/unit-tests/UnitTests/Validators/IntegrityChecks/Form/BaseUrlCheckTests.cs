using System.Linq;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class BaseUrlCheckTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContext = new();

        [Fact]
        public void Validate_ShouldAddFailureMessage_WhenBaseUrlEmpty()
        {
            // Arrange
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(_ => _.Request.Path)
                .Returns("/test-base-url");

            _mockHttpContext.Setup(_ => _.HttpContext)
                .Returns(mockContext.Object);

            var baseUrlCheck = new BaseUrlCheck(_mockHttpContext.Object);

            var schema = new FormSchemaBuilder()
                .WithName("form")
                .WithBaseUrl("")
                .Build();

            // Act
            var result = baseUrlCheck.Validate(schema);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.Equal("FAILURE - FormSchema BaseURL Check, BaseUrl property cannot be null or empty and needs to be the same as base request URL /test-base-url", result.Messages.First());
        }

        [Fact]
        public void Validate_ShouldAddFailureMessage_WhenBaseUrlDoesNotMatchPath()
        {
            // Arrange
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(_ => _.Request.Path)
                .Returns("/test-base-url");

            _mockHttpContext.Setup(_ => _.HttpContext)
                .Returns(mockContext.Object);

            var baseUrlCheck = new BaseUrlCheck(_mockHttpContext.Object);

            var schema = new FormSchemaBuilder()
                .WithName("form")
                .WithBaseUrl("base-url")
                .Build();

            // Act
            var result = baseUrlCheck.Validate(schema);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.Equal("FAILURE - FormSchema BaseURL Check, BaseUrl property within form schema needs to be the same as base request URL /test-base-url", result.Messages.First());
        }

        [Fact]
        public void Validate_ShouldNotAddFailureMessage_WhenBaseUrlMatchesPath()
        {
            // Arrange
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(_ => _.Request.Path)
                .Returns("/base-url");

            _mockHttpContext.Setup(_ => _.HttpContext)
                .Returns(mockContext.Object);

            var baseUrlCheck = new BaseUrlCheck(_mockHttpContext.Object);

            var schema = new FormSchemaBuilder()
                .WithName("form")
                .WithBaseUrl("base-url")
                .Build();

            // Act
            var result = baseUrlCheck.Validate(schema);

            // Assert
            Assert.Empty(result.Messages);
        }

        [Theory]
        [InlineData("/Preview")]
        [InlineData("/FB_PREVIEW_")]
        public void Validate_ShouldNotAddFailureMessage_ForPreviewPaths(string path)
        {
            // Arrange
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(_ => _.Request.Path)
                .Returns(path);

            _mockHttpContext.Setup(_ => _.HttpContext)
                .Returns(mockContext.Object);

            var baseUrlCheck = new BaseUrlCheck(_mockHttpContext.Object);

            var schema = new FormSchemaBuilder()
                .WithName("form")
                .Build();

            // Act
            var result = baseUrlCheck.Validate(schema);

            // Assert
            Assert.Empty(result.Messages);
        }

        
        [Fact]
        public void Validate_ShouldNotAddFailureMessage_ForDocuemtDownload()
        {
            // Arrange
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(_ => _.Request.Path)
                .Returns("/document/Summary/Txt/8cf44c49-1ade-471f-87e6-b30f96478197");

            _mockHttpContext.Setup(_ => _.HttpContext)
                .Returns(mockContext.Object);

            var baseUrlCheck = new BaseUrlCheck(_mockHttpContext.Object);

            var schema = new FormSchemaBuilder()
                .WithName("form")
                .Build();

            // Act
            var result = baseUrlCheck.Validate(schema);

            // Assert
            Assert.Empty(result.Messages);
        }
    }
}
