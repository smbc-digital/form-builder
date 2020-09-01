using form_builder.Conditions;
using form_builder.Enum;
using form_builder.Models;
using form_builder_tests.Builders;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Conditions
{
    public class AnyConditionTests
    {
        private readonly ConditionValidator conditionValidator = new ConditionValidator();

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void AnyCondition_ShouldReturnTrue_When_X_NumberOfConditions_AreTrue(int count)
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test", "pear"},
                {"test2", "apple"},
                {"test3", "cherry"},
                {"test4", "orange"},
            };

            var conditionOne = new ConditionBuilder()
                .WithConditionType(ECondition.EqualTo)
                .WithQuestionId("test")
                .WithComparisonValue("pear")
                .Build();

            var conditionTwo = new ConditionBuilder()
                .WithConditionType(ECondition.EqualTo)
                .WithQuestionId("test2")
                .WithComparisonValue("apple")
                .Build();

            var conditionThree = new ConditionBuilder()
                .WithConditionType(ECondition.Contains)
                .WithQuestionId("test3")
                .WithComparisonValue("cherry")
                .Build();

            var conditionFour = new ConditionBuilder()
                .WithConditionType(ECondition.Contains)
                .WithQuestionId("test4")
                .WithComparisonValue("orange")
                .Build();

            var anyCondition = new ConditionBuilder()
                .WithComparisonValue($"{count}")
                .WithConditionType(ECondition.Any)
                .WithCondition(conditionOne)
                .WithCondition(conditionTwo)
                .WithCondition(conditionThree)
                .WithCondition(conditionFour)
                .Build();

            // Act & Assert
            Assert.True(conditionValidator.IsValid(anyCondition, viewModel));
        }

        [Fact]
        public void AnyCondition_ShouldReturnFalse_When_X_NumberOfConditions_DoNotMatch()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test", "cat"},
                {"test2", "dog"},
                {"test3", "rabbit"},
                {"test4", "fish"},
            };

            var conditionOne = new ConditionBuilder()
                .WithConditionType(ECondition.EqualTo)
                .WithQuestionId("test")
                .WithComparisonValue("pear")
                .Build();

            var conditionTwo = new ConditionBuilder()
                .WithConditionType(ECondition.EqualTo)
                .WithQuestionId("test2")
                .WithComparisonValue("apple")
                .Build();

            var conditionThree = new ConditionBuilder()
                .WithConditionType(ECondition.Contains)
                .WithQuestionId("test3")
                .WithComparisonValue("cherry")
                .Build();

            var conditionFour = new ConditionBuilder()
                .WithConditionType(ECondition.Contains)
                .WithQuestionId("test4")
                .WithComparisonValue("orange")
                .Build();

            var anyCondition = new ConditionBuilder()
                .WithComparisonValue("1")
                .WithConditionType(ECondition.Any)
                .WithCondition(conditionOne)
                .WithCondition(conditionTwo)
                .WithCondition(conditionThree)
                .WithCondition(conditionFour)
                .Build();

            // Act & Assert
            Assert.False(conditionValidator.IsValid(anyCondition, viewModel));
        }
    }
}