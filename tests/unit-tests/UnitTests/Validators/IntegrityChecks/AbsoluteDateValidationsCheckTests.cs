using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class AbsoluteDateValidationsCheckTests
    {
        [Fact]
        public void AbsoluteDateValidationsCheck_Returns_IsValid_False_IsDateBeforeAbsolute_Contains_InvalidDate()
        {
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithIsDateBeforeAbsolute("test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();
            
            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-form")
                .Build();

            var check = new AbsoluteDateValidationsCheck();

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Equal($"FAILURE - Absolute Date Validations Check, IsDateBeforeAbsolute validation, 'test-date' does not provide a valid comparison date in form 'test-form'", result.Messages.First());
        }

        [Fact]
        public void AbsoluteDateValidationsCheck_Returns_IsValid_False_IsDateAfterAbsolute_Contains_InvalidDate()
        {
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithIsDateAfterAbsolute("test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();
            
            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-form")
                .Build();

            var check = new AbsoluteDateValidationsCheck();

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Equal($"FAILURE - Absolute Date Validations Check, IsDateAfterAbsolute validation, 'test-date' does not provide a valid comparison date in form 'test-form'", result.Messages.First());
        }
    }
}