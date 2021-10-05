using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        [InlineData("/v2/form-name", "/form-name", 302)]
        [InlineData("/V2/form-name", "/form-name", 302)]
        public async Task Invoke_ShouldReturnRedirectStatusCode_ForLegacyPathPrefix(string actualPath, string expectedPath, int statusCode)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = actualPath;

            // Act
            await _legacyRedirect.Invoke(httpContext);

            // Assert
            Assert.Equal(statusCode, httpContext.Response.StatusCode);
            Assert.Equal(expectedPath, httpContext.Response.Headers["Location"][0]);
        }

        [Theory]
        [InlineData("/form-name", 200)]
        [InlineData("/v2-form-name", 200)]
        public async Task Invoke_ShouldReturnOkStatusCode_ForNonLegacyPathPrefix(string actualPath, int statusCode)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = actualPath;

            // Act
            await _legacyRedirect.Invoke(httpContext);

            // Assert
            Assert.Equal(statusCode, httpContext.Response.StatusCode);
            Assert.Empty(httpContext.Response.Headers);
        }

    }
}
