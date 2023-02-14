using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class KeyCheckTests
    {
        private readonly KeyCheck _keyCheck = new();

        [Fact]
        public void Validate_ShouldAddFailureMessage_If_KeyNameIsNotSpecified_But_KeyValueIsSpecified()
        {
            var schema = new FormSchemaBuilder()
                .WithFormAccessKeyValue("TestKey")
                .Build();

            // Act
            var result = _keyCheck.Validate(schema);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.StartsWith("FAILURE - FormSchema Key Check, 'KeyName'", result.Messages.First());
        }

        [Fact]
        public void Validate_ShouldAddFailureMessage_If_Key_IsNot_Specified()
        {
            var schema = new FormSchemaBuilder()
                .WithKeyName("TestKeyName")
                .Build();

            // Act
            var result = _keyCheck.Validate(schema);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.StartsWith("FAILURE - FormSchema Key Check, 'Key", result.Messages.First());
        }
    }
}
