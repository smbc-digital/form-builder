using form_builder.Helpers.Cookie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class CookieHelperTests
    {
        private readonly CookieHelper _helper;
        private readonly Mock<IHttpContextAccessor> _mockHttpContext = new ();
        private string _CookieKey => "cookie-key";
        private string _CookieValue => "stored-cookie-value";
        public CookieHelperTests()
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();

            var mockContext = new Mock<HttpContext>();
            var requestFeature = new HttpRequestFeature();
            var featureCollection = new FeatureCollection();
            requestFeature.Headers = new HeaderDictionary();
            requestFeature.Headers.Add("cookie", new StringValues(_CookieKey + "=" + _CookieValue));
            featureCollection.Set<IHttpRequestFeature>(requestFeature);
            var cookiesFeatureRequest = new RequestCookiesFeature(featureCollection);

            mockContext.Setup(_ => _.Request.Cookies)
                .Returns(cookiesFeatureRequest.Cookies);

            _mockHttpContext.Setup(_ => _.HttpContext)
                .Returns(mockContext.Object);

            _helper = new CookieHelper(_mockHttpContext.Object);
        }

        [Fact]
        public void GetCookie_ShouldReturn_Cookie_Value()
        {
            // Act
            var result = _helper.GetCookie(_CookieKey);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_CookieValue, result);
        }
    }
}