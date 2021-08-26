using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Lookup;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class DynamicLookupCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();
        private readonly DynamicLookupCheck _integrityCheck;

        public DynamicLookupCheckTests() => 
            _integrityCheck = new DynamicLookupCheck(_mockHostingEnv.Object, new List<ILookupProvider>());

        [Theory]
        [InlineData("local", "int", "Fake", "TestToken", "https://myapi.com")] // 1) No Match:  environment for current Environment Name
        [InlineData("local", "", "Fake", "TestToken", "https://myapi.com")] // 2) No Environment Name
        [InlineData("local", "local", "", "TestToken", "https://myapi.com")] // 3) No Provider Name
        [InlineData("local", "local", "Test_Provider", "TestToken", "https://myapi.com")] // 4) No Provider found.. ( Just have Fake to select )
        [InlineData("local", "local", "Fake", "TestToken", "")] // 5) No URL to hit
        [InlineData("local", "local", "Fake", "", "https://myapi.com")] // 6) No Auth Token
        [InlineData("int", "int", "Fake", "TestToken", "http://myapi.com")] // 7) No https if not on local
        public void ValidateDynamicLookUpObject_ShouldThrowError_IfNotValidLookupProperties(string environmentName, string lookupEnv, string provider, string authToken, string url)
        {
            // Arrange
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns(environmentName);

            var element = new ElementBuilder().WithType(EElementType.Radio).WithLookup("dynamic").Build();
            element.Properties.LookupSources = new List<LookupSource>
            {
                new LookupSource
                {
                    EnvironmentName = lookupEnv,
                    Provider = provider,
                    AuthToken = authToken,
                    URL = url
                }
            };

            var page = new PageBuilder().WithElement(element).Build();
            List<Page> pages = new() { page };

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_LookupIs_Dynamic_ButHas_NoSources()
        {
            // Arrange
            var questionId = "testQuestionId";
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("testEnv");

            var element = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId(questionId)
                .WithLookup("dynamic")
                .Build();

            var page = new PageBuilder().WithElement(element).Build();
            List<Page> pages = new() { page };

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal($"FAILURE - Dynamic Lookup Check, dynamic lookup with questionId {questionId} requires a LookupSource", result.Messages.First());
        }
    }
}
