using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Helpers.DocumentCreation;
using form_builder.Models;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class DocumentCreationHelperTests
    {
        private readonly DocumentCreationHelper _documentCreation;
        public DocumentCreationHelperTests()
        {
            _documentCreation = new DocumentCreationHelper();
        }

        [Theory]
        [InlineData(EElementType.Textbox, "question label", "test")]
        [InlineData(EElementType.Textarea, "What is your", "textAreaValue")]
        [InlineData(EElementType.Checkbox, "Checkbox label", "yes")]
        [InlineData(EElementType.DatePicker, "Enter the date", "01/01/2000")]
        [InlineData(EElementType.Radio, "Radio radio", "no")]
        public void GenerateQuestionAndAnswersDictionary_ShouldReturnCorrectLabelText_ForElements(EElementType type, string labelText, string value)
        {
            var questionId = "test-questionID";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = value } } }}};

            var element = new ElementBuilder()
                            .WithType(type)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value}", result[0]);
        }

        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldReturnCorrectLabelText_ForStreetElement()
        {
            var questionId = "test-questionID";
            var labelText = "Enter the Street";
            var value = "street, city, postcode, uk";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-streetaddress-description", Response = value } } }}};

            var element = new ElementBuilder()
                            .WithType(EElementType.Street)
                            .WithQuestionId(questionId)
                            .WithStreetLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value}", result[0]);
        }

        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldReturnCorrectLabelText_ForAddressElement()
        {
            var questionId = "test-questionID";
            var labelText = "Whats your Address";
            var value = "11 road, city, postcode, uk";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-address-description", Response = value } } }}};

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .WithAddressLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value}", result[0]);
        }
    }
}
