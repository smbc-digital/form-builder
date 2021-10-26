using System.Threading.Tasks;
using System.Web;
using form_builder.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Middleware
{
    public class LegacyRedirectsTests
    {
        private readonly LegacyRedirect _legacyRedirect;
        public LegacyRedirectsTests()
        {
            var next = new Mock<RequestDelegate>();
            _legacyRedirect = new LegacyRedirect(next.Object);
        }

        [Theory]
        [InlineData("/v2/form-name", "/form-name")]
        [InlineData("/V2/form-name", "/form-name")]
        [InlineData("/V2/form-name?querystring=test", "/form-name?querystring=test")]
        [InlineData("/v2/form-name?querystring=test&another=one", "/form-name?querystring=test&another=one")]
        public async Task Invoke_ShouldReturnRedirectStatusCode_ForLegacyPathPrefix(string actualPath, string expectedPath)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = actualPath;

            // Act
            await _legacyRedirect.Invoke(httpContext);

            // Assert
            Assert.Equal(302, httpContext.Response.StatusCode);
            Assert.Equal(expectedPath, HttpUtility.UrlDecode(httpContext.Response.Headers["Location"][0]));
        }

        [Theory]
        [InlineData("/form-name")]
        [InlineData("/v2-form-name")]
        [InlineData("/form-name?querystring=test")]
        [InlineData("/form-name?querystring=test&another=one")]
        public async Task Invoke_ShouldReturnOkStatusCode_ForNonLegacyPathPrefix(string actualPath)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = actualPath;

            // Act
            await _legacyRedirect.Invoke(httpContext);

            // Assert
            Assert.Equal(200, httpContext.Response.StatusCode);
            Assert.Empty(httpContext.Response.Headers);
        }
    }
}
