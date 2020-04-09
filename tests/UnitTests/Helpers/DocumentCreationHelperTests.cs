using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.DocumentCreation;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class DocumentCreationHelperTests
    {
        private readonly DocumentCreationHelper _documentCreation;
        private readonly Mock<IElementMapper> _mockElementMapper = new Mock<IElementMapper>();

        public DocumentCreationHelperTests()
        {
            _documentCreation = new DocumentCreationHelper(_mockElementMapper.Object);
        }


        [Fact]
        public void GenerateQuestionAndAnswersList_ShouldReturn_List_Without_NonValidatable_Elements()
        {
            _mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns("test value");
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};
            var labelText = "I am a label";

            var element = new ElementBuilder()
                            .WithType(EElementType.H1)
                            .Build();

            var element2 = new ElementBuilder()
                            .WithType(EElementType.P)
                            .Build();

            var element3 = new ElementBuilder()
                            .WithType(EElementType.Textarea)
                            .WithQuestionId("QuestiionId")
                            .WithLabel("I am a label")
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .WithElement(element2)
                        .WithElement(element3)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: test value", result[0]);
        }


        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.Checkbox)]
        [InlineData(EElementType.Radio)]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.DatePicker)]
        [InlineData(EElementType.Select)]
        [InlineData(EElementType.TimeInput)]
        public void GenerateQuestionAndAnswersList_ShouldReturn_ListWithSingleItem_For_ElementType(EElementType type)
        {
            var value = "value";
            _mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .Returns(value);

            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};
            var labelText = "I am a label";

            var element = new ElementBuilder()
                            .WithType(type)
                            .WithQuestionId("testQuestion")
                            .WithLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value}", result[0]);
        }

        [Fact]
        public void GenerateQuestionAndAnswersList_ShouldReturn_ListOfMultipleItems()
        {
            var value = "value";
            _mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .Returns(value);

            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};
            var labelText = "I am a label";
            var labelText2 = "second label";
            var labelText3 = "third label";

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("testQuestion1")
                .WithLabel(labelText)
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("testQuestion2")
                .WithLabel(labelText2)
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithQuestionId("testQuestion3")
                .WithLabel(labelText3)
                .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .WithElement(element2)
                        .WithElement(element3)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Equal(3, result.Count);
            Assert.Equal($"{labelText}: {value}", result[0]);
            Assert.Equal($"{labelText2}: {value}", result[1]);
            Assert.Equal($"{labelText3}: {value}", result[2]);
        }
    }
}