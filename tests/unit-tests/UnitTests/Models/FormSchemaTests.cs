using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models
{
    public class FormSchemaTests
    {
        private readonly Mock<IPageHelper> _mockPageHelper = new();

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenNoEnvironmentAvailabilitiesAreSpecified()
        {
            // Arrange
            var formSchema = new FormSchema();

            // Act
            var result = formSchema.IsAvailable("Int");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenRequestedEnvironmentAvailabilitiesIsNotSpecified()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("prod", false)
                .Build();

            // Act
            var result = formSchema.IsAvailable("Int");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenRequestedEnvironmentAvailabilitiesIsSpecified_And_IsAvailableEqualsTrue()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("Int", true)
                .Build();

            // Act
            var result = formSchema.IsAvailable("Int");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_False_WhenRequestedEnvironmentAvailabilitiesIsSpecified_And_IsAvailableEqualsFalse()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("Int", false)
                .Build();

            // Act
            var result = formSchema.IsAvailable("Int");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPage_ShouldThrowException_If_PageIsNull()
        {
            // Arrange
            var formSchema = new FormSchema
            {
                EnvironmentAvailabilities = new List<EnvironmentAvailability>
                {
                    new EnvironmentAvailability {
                        Environment = "Int",
                        IsAvailable = false
                    }
                }
            };

            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns((Page)null);

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() => formSchema.GetPage(_mockPageHelper.Object, "path"));
            Assert.Contains("Requested path 'path' object could not be found or was not unique", result.Message);
        }

        [Fact]
        public void GetPage_ShouldReturnPageWithMatchingConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "testRadio",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "testRadio",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "no"
                })
                .WithRenderConditions(new Condition
                {
                    QuestionId = "testInput",
                    ConditionType = ECondition.EqualTo,
                    ComparisonDate = "test"
                })
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("baseUrl")
                .WithName("form name")
                .WithStartPageUrl("page1")
                .WithPage(page)
                .WithPage(page2)
                .Build();

            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(page);

            // Act
            var result = formSchema.GetPage(_mockPageHelper.Object, "success");

            // Assert
            Assert.Equal(page, result);
        }

        [Fact]
        public void GetPage_ShouldReturnPageWithNoMatchingConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "testRadio",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "no"
                })
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("baseUrl")
                .WithName("form name")
                .WithStartPageUrl("page1")
                .WithPage(page)
                .WithPage(page2)
                .Build();

            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(page);

            // Act
            var result = formSchema.GetPage(_mockPageHelper.Object, "success");

            // Assert
            Assert.Equal(page, result);
        }
    }
}