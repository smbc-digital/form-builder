using form_builder.Configuration;
using form_builder.Restrictions;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Restrictions
{
    public class KeyFormAccessRestrictionsTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
        private readonly Mock<IOptions<QAFormAccessTokenConfiguration>> _mockOptions = new();

        private readonly string _qaAccessToken = Guid.NewGuid().ToString();

        public KeyFormAccessRestrictionsTests()
        {
            _mockOptions
                .Setup(_ => _.Value)
                .Returns(new QAFormAccessTokenConfiguration { AccessKey = _qaAccessToken });
        }

        [Fact]
        public void IsRestricted_Should_ReturnFalse_If_NoKeyIsSpecified()
        {
            var formSchema = new FormSchemaBuilder().Build();
            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object, _mockOptions.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }

        [Fact]
        public void IsRestricted_Should_ReturnTrue_If_KeyIsSpecified_And_HttpContextContainsNoValues()
        {
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

            _mockHttpContextAccessor
                .Setup(_ => _.HttpContext.Request.Query)
                .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
                .WithSpecifiedFormAccessKey("TestKey", "TestToken")
                .Build();

            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object, _mockOptions.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void IsRestricted_Should_ReturnTrue_If_KeyIsSpecified_And_HttpContextContainsIncorrectValues()
        {
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
            {
                { "TestKey", "IncorrectToken" }
            });

            _mockHttpContextAccessor
                .Setup(_ => _.HttpContext.Request.Query)
                .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
                .WithSpecifiedFormAccessKey("TestKey", "TestToken")
                .Build();

            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object, _mockOptions.Object);
            var result = restriction.IsRestricted(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void IsRestricted_Should_ReturnFalse_If_KeyIsSpecified_And_HttpContextContainsCorrectValues()
        {
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
            {
                { "TestKey", "TestToken" }
            });

            _mockHttpContextAccessor
                .Setup(_ => _.HttpContext.Request.Query)
                .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
                .WithSpecifiedFormAccessKey("TestKey", "TestToken")
                .Build();

            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object, _mockOptions.Object);
            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }

        [Fact]
        public void IsRestricted_Should_ReturnFalse_If_KeyIsSpecified_And_HttpContextContainsCorrectQAFormAccessToken()
        {
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
            {
                { "TestKey", _qaAccessToken}
            });

            _mockHttpContextAccessor
                .Setup(_ => _.HttpContext.Request.Query)
                .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
                .WithSpecifiedFormAccessKey("TestKey", "TestToken")
                .Build();

            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object, _mockOptions.Object);
            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }
    }
}