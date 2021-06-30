using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Services.MappingService.Entities;
using form_builder.TagParsers.Formatters;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class ActionHelperTests
    {
        private const string validResponse = "ValidResponse";
        private const string validVariableQuestionId = "validVariableQuestionId";
        private const string validVariableQuestionIdInBraces = "{{validVariableQuestionId}}";
        private const string invalidVariableQuestionIdInBraces = "{{invalidVariableQuestionId}}";
        private const string contentWithValidVariableQuestionIdInBraces = "Some text with a {{validVariableQuestionId}}.";
        private const string contentWithInvalidVariableQuestionIdInBraces = "Some text with a {{invalidVariableQuestionId}}";

        private readonly IActionHelper _actionHelper;
        private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new ();

        private readonly MappingEntity _mappingEntity = new MappingEntityBuilder()
            .WithFormAnswers(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>
                    {
                        new PageAnswers
                        {
                            Answers = new List<Answers>
                            {
                                new Answers
                                {
                                    Response = "testResponse",
                                    QuestionId = "testQuestionId"
                                }
                            },
                            PageSlug = "page-one"
                        },
                        new PageAnswers
                        {
                            Answers = new List<Answers>
                            {
                                new Answers
                                {
                                    Response = "testResponse.email@test.com",
                                    QuestionId = "emailQuestion"
                                },
                                new Answers
                                {
                                    Response = validResponse,
                                    QuestionId = validVariableQuestionId
                                }
                            },
                            PageSlug = "page-one"
                        }
                    }
            })
            .Build();

        private readonly FormSchema _formSchema = new FormSchemaBuilder()
            .WithStartPageUrl("page-one")
            .WithBaseUrl("base-test")
            .WithPage(new PageBuilder()
            .WithElement(new ElementBuilder()
            .WithType(EElementType.H2)
            .Build())
            .WithPageSlug("success")
            .Build())
            .WithFormActions(new UserEmail
            {
                Properties = new BaseActionProperty
                {
                    To = "email.test@test.com, {{emailQuestion}}"
                },
                Type = EActionType.UserEmail
            })
            .Build();

        public ActionHelperTests() => _actionHelper = new ActionHelper(_mockFormatters.Object);

        [Fact]
        public void GenerateUrl_ShouldGenerateCorrectGetUrl_PathParameters()
        {
            // Act
            var result = _actionHelper.GenerateUrl("www.testurl.com/{{testQuestionId}}", _mappingEntity.FormAnswers);

            // Assert
            Assert.Equal("www.testurl.com/testResponse", result.Url);
            Assert.False(result.IsPost);
        }

        [Fact]
        public void GenerateUrl_ShouldGenerateCorrectGetUrl_QueryStringParameters()
        {
            // Act
            var result = _actionHelper.GenerateUrl("www.testurl.com?id={{testQuestionId}}", _mappingEntity.FormAnswers);

            // Assert
            Assert.Equal("www.testurl.com?id=testResponse", result.Url);
            Assert.False(result.IsPost);
        }

        [Fact]
        public void GenerateUrl_ShouldGenerateCorrectPostUrl()
        {
            // Act
            var result = _actionHelper.GenerateUrl("www.testurl.com", _mappingEntity.FormAnswers);

            // Assert
            Assert.Equal("www.testurl.com", result.Url);
            Assert.True(result.IsPost);
        }

        [Fact]
        public void GetEmailToAddresses_ShouldReturnListOfToEmailAddress()
        {
            // Act
            var result = _actionHelper.GetEmailToAddresses(_formSchema.FormActions.FirstOrDefault(), _mappingEntity.FormAnswers);

            // Assert
            Assert.Equal("testResponse.email@test.com,email.test@test.com", result);
        }

        [Theory]
        [InlineData(contentWithValidVariableQuestionIdInBraces)]
        [InlineData(contentWithInvalidVariableQuestionIdInBraces)]
        public void GetEmailContent_ShouldReturnContent_WithQuestionResponse_Or_EmptyString(string content)
        {
            // Arrange
            var action = _formSchema.FormActions.FirstOrDefault();
            action.Properties.Content = content;

            // Act
            var result = _actionHelper.GetEmailContent(action, _mappingEntity.FormAnswers);

            // Assert
            Assert.True(result.Contains(validResponse) && !result.Contains(validVariableQuestionIdInBraces) || !result.Contains(invalidVariableQuestionIdInBraces));
        }
    }
}