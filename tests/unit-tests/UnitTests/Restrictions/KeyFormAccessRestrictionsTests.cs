using form_builder.Restrictions;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class KeyFormAccessRestrictionsTests
    {

        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();

        [Fact]
        public void IsRestricted_ShouldReturn_False_If_No_Key_Is_Specified()
        {
            var formSchema = new FormSchemaBuilder().Build();
            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }

        [Fact]
        public void IsRestricted_ShouldReturn_True_If_Key_Is_Specified_And_HttpContext_Contains_No_Values()
        {
            var mockHttpContext = new Mock<HttpContent>();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Query)
                                    .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
            .WithSpecifiedKey("TestKey", "TestToken")
            .Build();

            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object);

            var result = restriction.IsRestricted(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void IsRestricted_ShouldReturn_True_If_Key_Is_Specified_And_HttpContext_Contains_Incorrect_Values()
        {
            var mockHttpContext = new Mock<HttpContent>();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
            {
                { "TestKey", "IncorrectToken" }
            });

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Query)
                             .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
            .WithSpecifiedKey("TestKey", "TestToken")
            .Build();

            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object);
            var result = restriction.IsRestricted(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void IsRestricted_ShouldReturn_False_If_Key_Is_Specified_And_HttpContext_Contains_Correct_Values()
        {
            var mockHttpContext = new Mock<HttpContent>();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
            {
                { "TestKey", "TestToken" }
            });

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Query)
                             .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
            .WithSpecifiedKey("TestKey", "TestToken")
            .Build();


            KeyFormAccessRestriction restriction = new(_mockHttpContextAccessor.Object);
            var result = restriction.IsRestricted(formSchema);

            Assert.False(result);
        }
    }
}