using form_builder.Builders;
using form_builder.Enum;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class ElementTests
    {

        [Theory]
        [InlineData(EElementType.Checkbox)]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.DatePicker)]
        [InlineData(EElementType.MultipleFileUpload)]
        [InlineData(EElementType.Radio)]
        [InlineData(EElementType.Select)]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.TimeInput)]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_IsDefined(EElementType type)
        {
            var label = "Custom label";
            //Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("test-question-id")
                .WithSummaryLabel(label)
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal(label, result);
        }

        [Theory]
        [InlineData(EElementType.Checkbox)]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.DatePicker)]
        [InlineData(EElementType.MultipleFileUpload)]
        [InlineData(EElementType.Radio)]
        [InlineData(EElementType.Select)]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.TimeInput)]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_IsDefined_AndOptional(EElementType type)
        {
            var label = "Custom label";
            //Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("test-question-id")
                .WithSummaryLabel(label)
                .WithOptional(true)
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal($"{label} (optional)", result);
        }

        [Theory]
        [InlineData(EElementType.Checkbox)]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.DatePicker)]
        [InlineData(EElementType.MultipleFileUpload)]
        [InlineData(EElementType.Radio)]
        [InlineData(EElementType.Select)]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.TimeInput)]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_Is_Not_Defined(EElementType type)
        {
            var label = "existing label";
            //Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("test-question-id")
                .WithLabel(label)
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal(label, result);
        }

        [Theory]
        [InlineData(EElementType.Checkbox)]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.DatePicker)]
        [InlineData(EElementType.MultipleFileUpload)]
        [InlineData(EElementType.Radio)]
        [InlineData(EElementType.Select)]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.TimeInput)]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_Is_Not_Defined_And_Optional(EElementType type)
        {
            var label = "existing label";
            //Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("test-question-id")
                .WithLabel(label)
                .WithOptional(true)
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal($"{label} (optional)", result);
        }
    }
}
