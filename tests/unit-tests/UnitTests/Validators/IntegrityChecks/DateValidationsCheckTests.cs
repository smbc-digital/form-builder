using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class DateValidationsCheckTests
    {
        [Fact]
        public void CheckDateValidations_Throw_ApplicationException_IsDateBefore_Contains_InvalidQuestionId()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithIsDateBefore("test-comparison")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();
            
            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var check = new DateValidationsCheck();

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Equal($"FAILURE - Date Validations Check, IsDateBefore validation, for question 'test-date' - the form does not contain a comparison element with question id 'test-comparison'", result.Messages.First());
        }

        [Fact]
        public void CheckDateValidations_Throw_ApplicationException_IsDateAfter_Contains_InvalidQuestionId()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithIsDateAfter("test-comparison")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var check = new DateValidationsCheck();

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Equal("FAILURE - Date Validations Check, IsDateAfter validation, for question 'test-date' - the form does not contain a comparison element with QuestionId 'test-comparison'", result.Messages.First());
        }
    }
}