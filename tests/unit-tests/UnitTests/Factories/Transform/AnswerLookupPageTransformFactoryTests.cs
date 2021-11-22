using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Models;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class AnswerLookupPageTransformFactoryTests
    {
        private AnswerLookupPageTransformFactory _answerLookupPageTransformFactory = new();

        [Theory]
        [InlineData("#dynamic")]
        [InlineData("#local")]
        [InlineData("#S3")]
        public async Task Transform_Should_Transform(string lookUp)
        {
            // Arrange
            string questionId = "testQuestionId";
            List<Option> options = new() { new() { Text = "This", Value = "This" }, new() { Text = "That", Value = "That" } };
            FormAnswers convertedAnswers = new()
            {
                Pages = new()
                {
                    new()
                    {
                        Answers = new()
                        {
                            new(lookUp.TrimStart('#'), JsonSerializer.Serialize(options))
                        }
                    }
                }
            };

            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithLookup(lookUp)
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _answerLookupPageTransformFactory.Transform(page, convertedAnswers);

            // Assert
            var elementToTest = page.Elements.Single(element => element.Properties.QuestionId.Equals(questionId));
            Assert.True(elementToTest.Properties.Options.Count.Equals(options.Count));
        }

        [Theory]
        [InlineData("dynamic")]
        [InlineData("local")]
        [InlineData("S3")]
        public async Task Transform_Should_Not_Transform(string lookUp)
        {
            // Arrange
            string questionId = "testQuestionId";
            List<Option> options = new() { new() { Text = "This", Value = "This" }, new() { Text = "That", Value = "That" } };
            FormAnswers convertedAnswers = new()
            {
                Pages = new()
                {
                    new()
                    {
                        Answers = new()
                        {
                            new(lookUp.TrimStart('#'), JsonSerializer.Serialize(options))
                        }
                    }
                }
            };

            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithLookup(lookUp)
                .WithType(EElementType.Checkbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            await _answerLookupPageTransformFactory.Transform(page, convertedAnswers);

            // Assert
            // page element has no options
            var elementToTest = page.Elements.Single(element => element.Properties.QuestionId.Equals(questionId));
            Assert.True(elementToTest.Properties.Options.Count.Equals(0));
        }
    }
}