using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class HasDuplicateQuestionIdsCheckTests
    {
        [Fact]
        public void HasDuplicateQuestionIDsCheck_IsNotValid_IfDuplicateQuestionIDsInJSON()
        {
            // Arrange
            var element1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox1")
                .WithLabel("First name")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox2")
                .WithLabel("Middle name")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox1")
                .WithLabel("First name")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element3)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page1)
                .WithPage(page2)
                .WithName("test-name")
                .Build();

            var check = new HasDuplicateQuestionIdsCheck();

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void HasDuplicateQuestionIDsCheck_IsValid_IfNoDuplicateQuestionIDsInJSON()
        {
            // Arrange
            var element1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox1")
                .WithLabel("First name")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox2")
                .WithLabel("Middle name")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox3")
                .WithLabel("First name")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element3)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page1)
                .WithPage(page2)
                .WithName("test-name")
                .Build();

            var check = new HasDuplicateQuestionIdsCheck();

            // Act
            var result = check.Validate(schema);
            Assert.True(result.IsValid);
        }
    }
}