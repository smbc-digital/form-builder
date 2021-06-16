using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
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

            // Act
            HasDuplicateQuestionIdsCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void HasDuplicateQuestionIDsCheck_IsNotValid_IfDuplicateNestedQuestionIDsInJSON()
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
                .WithType(EElementType.AddAnother)
                .WithQuestionId("addAnother")
                .WithNestedElement(element1)
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

            // Act
            HasDuplicateQuestionIdsCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
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


            // Act
            var check = new HasDuplicateQuestionIdsCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}