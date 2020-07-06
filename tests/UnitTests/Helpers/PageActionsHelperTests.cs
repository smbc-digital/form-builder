using System.Collections.Generic;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class PageActionsHelperTests
    {
        private readonly IPageActionsHelper _pageActionsHelper;

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
                        }
                    }
                })
                .Build();
        public PageActionsHelperTests()
        {
            _pageActionsHelper = new PageActionsHelper();
        }

        [Fact]
        public void GenerateUrl_ShouldGenerateCorrectGetUrl_PathParameters()
        {
            // Act
            var result = _pageActionsHelper.GenerateUrl("www.testurl.com/{{testQuestionId}}", _mappingEntity.FormAnswers);

            // Assert
            Assert.Equal("www.testurl.com/testResponse", result.Url);
            Assert.False(result.IsPost);
        }

        [Fact]
        public void GenerateUrl_ShouldGenerateCorrectGetUrl_QueryStringParameters()
        {
            // Act
            var result = _pageActionsHelper.GenerateUrl("www.testurl.com?id={{testQuestionId}}", _mappingEntity.FormAnswers);

            // Assert
            Assert.Equal("www.testurl.com?id=testResponse", result.Url);
            Assert.False(result.IsPost);
        }

        [Fact]
        public void GenerateUrl_ShouldGenerateCorrectPostUrl()
        {
            // Act
            var result = _pageActionsHelper.GenerateUrl("www.testurl.com", _mappingEntity.FormAnswers);

            // Assert
            Assert.Equal("www.testurl.com", result.Url);
            Assert.True(result.IsPost);
        }
    }
}
