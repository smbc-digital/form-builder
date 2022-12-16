using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class KeyCheckTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContext = new();

        [Fact]
        public void Validate_ShouldAddFailureMessage_KeyName_IsNot_Specified()
        {
            var keyCheck = new KeyCheck();

            var schema = new FormSchemaBuilder()
                .WithKey("TestKey")
                .Build();

            // Act
            var result = keyCheck.Validate(schema);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.StartsWith("FAILURE - FormSchema Key Check, 'KeyName'", result.Messages.First());
        }

        [Fact]
        public void Validate_ShouldAddFailureMessage_If_Key_IsNot_Specified()
        {
            var keyCheck = new KeyCheck();

            var schema = new FormSchemaBuilder()
                .WithKeyName("TestKeyName")
                .Build();

            // Act
            var result = keyCheck.Validate(schema);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.StartsWith("FAILURE - FormSchema Key Check, 'Key", result.Messages.First());
        }
    }
}
