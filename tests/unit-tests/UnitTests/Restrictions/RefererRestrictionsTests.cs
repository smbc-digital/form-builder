using form_builder.Restrictions;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Restrictions
{
    public class RefererRestrictionsTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();

        [Fact]
        public void IsRestricted_Should_ReturnFalse_If_NoRefererIsSpecified()
        {
            var formSchema = new FormSchemaBuilder().Build();
            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }

        [Fact]
        public void IsRestricted_Should_ReturnTrue_If_ReferrersSpecified_And_NoValueInHttpContext()
        {
            _mockHttpContextAccessor
                .Setup(_ => _.HttpContext.Request.Headers.Referer)
                .Returns(StringValues.Empty);

            var formSchema = new FormSchemaBuilder()
                .AddFormAccessReferrer("test.co.uk")
                .AddFormAccessReferrer("live.co.uk")
                .Build();

            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void IsRestricted_Should_ReturnFalse_If_ReferrersSpecified_And_MatchingValueInHttpContext()
        {
            _mockHttpContextAccessor
                .Setup(_ => _.HttpContext.Request.Headers.Referer)
                .Returns(new StringValues("https://www.test.co.uk"));

            var formSchema = new FormSchemaBuilder()
                .AddFormAccessReferrer("test.co.uk")
                .AddFormAccessReferrer("live.co.uk")
                .Build();

            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }

        [Fact]
        public void IsRestricted_Should_ReturnTrue_If_ReferrersSpecified_And_MatchingValueNotInHttpContext()
        {
            _mockHttpContextAccessor
                .Setup(_ => _.HttpContext.Request.Headers.Referer)
                .Returns(new StringValues("https://www.NotValid.co.uk"));

            var formSchema = new FormSchemaBuilder()
                .AddFormAccessReferrer("test.co.uk")
                .AddFormAccessReferrer("live.co.uk")
                .Build();

            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void IsRestricted_Should_ReturnFalse_If_ReferrersSpecified_And_MatchingMultipleValueInHttpContext()
        {
            _mockHttpContextAccessor
                .Setup(_ => _.HttpContext.Request.Headers.Referer)
                .Returns(new StringValues(new [] {"https://www.NotValid.co.uk", "https://www.live.co.uk"}));

            var formSchema = new FormSchemaBuilder()
                .AddFormAccessReferrer("test.co.uk")
                .AddFormAccessReferrer("live.co.uk")
                .Build();

            RefererRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }
    }
}