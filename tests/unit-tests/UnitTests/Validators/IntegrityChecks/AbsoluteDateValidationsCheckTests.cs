using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class AbsoluteDateValidationsCheckTests
    {
        [Fact]
        public void AbsoluteDateValidationsCheck_Returns_IsValid_False_IsDateBeforeAbsolute_Contains_InvalidDate()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithIsDateBeforeAbsolute("test")
                .Build();

            AbsoluteDateValidationsCheck check = new();

            // Act
            var result = check.Validate(element);
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void AbsoluteDateValidationsCheck_Returns_IsValid_False_IsDateAfterAbsolute_Contains_InvalidDate()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithIsDateAfterAbsolute("test")
                .Build();

            AbsoluteDateValidationsCheck check = new();

            // Act
            var result = check.Validate(element);
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}