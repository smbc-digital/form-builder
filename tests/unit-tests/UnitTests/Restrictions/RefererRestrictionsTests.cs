using form_builder.Restrictions;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class RefererRestrictionsTests
    {

        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();

        [Fact]
        public void IsRestricted_ShouldReturn_False_If_No_Referer_Is_Specified()
        {
            var formSchema = new FormSchemaBuilder().Build();
            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }

        [Fact]
        public void IsRestricted_ShouldReturn_True_If_Referrers_Specified_But_NoValue_In_HttpContext()
        {
            var mockHttpContext = new Mock<HttpContent>();

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Headers.Referer)
                                    .Returns(StringValues.Empty);

            var formSchema = new FormSchemaBuilder()
            .AddReferrer("test.co.uk")
            .AddReferrer("live.co.uk")
            .Build();

            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void IsRestricted_ShouldReturn_False_If_Referrers_Specified_And_MathchingValue_In_HttpContext()
        {
            var mockHttpContext = new Mock<HttpContent>();

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Headers.Referer)
                                    .Returns(new StringValues("https://www.test.co.uk"));

            var formSchema = new FormSchemaBuilder()
            .AddReferrer("test.co.uk")
            .AddReferrer("live.co.uk")
            .Build();

            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }

        [Fact]
        public void IsRestricted_ShouldReturn_True_If_Referrers_Specified_And_MathchingValue_Not_In_HttpContext()
        {
            var mockHttpContext = new Mock<HttpContent>();

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Headers.Referer)
                                    .Returns(new StringValues("https://www.NotValid.co.uk"));

            var formSchema = new FormSchemaBuilder()
            .AddReferrer("test.co.uk")
            .AddReferrer("live.co.uk")
            .Build();

            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void IsRestricted_ShouldReturn_False_If_Referrers_Specified_And_MathchingMultipleValue_In_HttpContext()
        {
            var mockHttpContext = new Mock<HttpContent>();

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Headers.Referer)
                                    .Returns(new StringValues(new [] {"https://www.NotValid.co.uk", "https://www.live.co.uk"}));

            var formSchema = new FormSchemaBuilder()
            .AddReferrer("test.co.uk")
            .AddReferrer("live.co.uk")
            .Build();

            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }
    }
}