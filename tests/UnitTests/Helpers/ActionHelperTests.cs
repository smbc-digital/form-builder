using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Services.MappingService.Entities;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class ActionHelperTests
    {
        private readonly IActionHelper _actionHelper;

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
                    To = "email.test@test.com, {{emailQuestion}}",
                },
                Type = EActionType.UserEmail
            })
        .Build();

        public ActionHelperTests()
        {
            _actionHelper = new ActionHelper();
        }

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
            Assert.Equal("testResponse.email@test.com,email.test@test.com,", result);
        }
    }
}
